using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OneDrive.Sdk;
using Microsoft.OneDrive.Sdk.Authentication;
using Newtonsoft.Json;
using File = System.IO.File;

namespace tudu;

public class Sync
{
	//TODO: Figure this out.
	public static class OneDrive
	{
		public static string   AppID     = "24a8a6c0-f6ed-4c6f-84e3-162d411daa8a";
		public static string[] Scope     = { "onedrive.readwrite" };
		public static string   ReturnURL = "";

		public static void GetAuth()
		{
			var auth = new MsaAuthenticationProvider(AppID, ReturnURL, Scope);
		}
	}

	public class DropBox
	{
		private const string CLIENT_ID = "gqvtt2vmwwox4tk";
		private const string LOOPBACK = "http://127.0.0.1:52475/";

		private const string AUTHENTICATION_URL = "www.dropbox.com/oauth2/authorize";
		private const string TOKEN_URL = "https://api.dropboxapi.com/oauth2/token";

		private readonly Uri _redirectURI = new Uri($"{LOOPBACK}authorize");
		private readonly Uri _jsRedirectURI = new Uri($"{LOOPBACK}token");
		
		private string _accessToken;

		private HttpClient _client;

		public DropBox()
		{
			_accessToken = String.Empty;
			_client = new HttpClient();
		}

		public void Sync(string force = "")
		{
			string tokenResponseJson = Authenticate();
			var tokenResponse = JsonConvert.DeserializeObject<OAuth2Response>(tokenResponseJson);

			_accessToken = tokenResponse.access_token;
			using (var dbx = new DropboxClient(_accessToken))
			{
				ListFolderResult list = dbx.Files.ListFolderAsync(string.Empty).Result;
				Metadata? todoFile = list.Entries.FirstOrDefault(i => i.Name == "todo.txt"); //Server-todo.txt
				var todoFileInfo = new FileInfo(Serialisation.TODO_FILE); //Client-todo.txt
				
				//TODO: PLEASE don't do this. Try to figure out a logging and desync fixer system.
				//	I'm pretty sure any newer device will upload because the freshly made todo file is considered newer
				
				//If the server version is OLDER than ours
				if ((todoFile == null || DateTime.Compare(todoFile.AsFile.ServerModified, todoFileInfo.LastWriteTime) < 0 || force == "upload") && force != "download")
				{
					//Upload
					Console.WriteLine("Uploading to server!");
					using (var mem = new MemoryStream(File.ReadAllBytes(Serialisation.TODO_FILE)))
					{
						dbx.Files.UploadAsync("/todo.txt", WriteMode.Overwrite.Instance, body: mem);
					}
				}
				else //If the sever version is NEWER than ours
				{
					Console.WriteLine("Downloading from server!");
					using (var response = dbx.Files.DownloadAsync("/todo.txt"))
					{
						response.Wait();
						byte[]? downloadResult = response.Result.GetContentAsByteArrayAsync().Result;
						if (downloadResult == null)
							throw new Exception($"Could not download file!\n{response.Exception?.Message}");
						
						File.WriteAllBytes(Serialisation.TODO_FILE, downloadResult);
					}
				}
			}
		}

		private string Authenticate()
		{
			var pkce = new Pkce();
			// string url = "https://" + authUrl + "?client_id=" + clientId + "&response_type=code&code_challenge=" + pkce.CodeChallenge + "&code_challenge_method=S256";
			string url = $"https://{AUTHENTICATION_URL}?client_id={CLIENT_ID}&response_type=code&code_challenge={pkce.CodeChallenge}";
			//TODO: Launch browser automatically and get response automatically.
			Common.OpenURL(url);
			Console.WriteLine("Please insert the following into your URL: " + url + "\nAnd insert back the code: ");
			
			string? auth = Console.ReadLine();
			if (auth == null) throw new Exception("You must input in your AUTH token!");
			
			var postreq = new Dictionary<string, string>()
			{
				{ "code",			auth },
				{ "grant_type",		"authorization_code" },
				{ "code_verifier",	pkce.CodeVerifier },
				{ "client_id",		CLIENT_ID },
			};
			var content = new FormUrlEncodedContent(postreq);
			
			//TODO: Use async
			HttpResponseMessage response = _client.PostAsync(TOKEN_URL, content).Result;
			string responseString = response.Content.ReadAsStringAsync().Result;
			return responseString;
		}
		
		
	}
}

/// <summary>
/// Provides a randomly generating PKCE code verifier and it's corresponding code challenge.
/// </summary>
public class Pkce
{
    /// <summary>
    /// The randomly generating PKCE code verifier.
    /// </summary>
    public string CodeVerifier;

    /// <summary>
    /// Corresponding PKCE code challenge.
    /// </summary>
    public string CodeChallenge;

    /// <summary>
    /// Initializes a new instance of the Pkce class.
    /// </summary>
    /// <param name="size">The size of the code verifier (43 - 128 charters).</param>
    public Pkce(uint size = 128) => NewCode(size);

	public void NewCode(uint size = 128)
	{
		CodeVerifier = GenerateCodeVerifier(size);
		CodeChallenge = GenerateCodeChallenge(CodeVerifier);
	}

    /// <summary>
    /// Generates a code_verifier based on rfc-7636.
    /// </summary>
    /// <param name="size">The size of the code verifier (43 - 128 charters).</param>
    /// <returns>A code verifier.</returns>
    /// <remarks> 
    /// code_verifier = high-entropy cryptographic random STRING using the 
    /// unreserved characters[A - Z] / [a-z] / [0-9] / "-" / "." / "_" / "~"
    /// from Section 2.3 of[RFC3986], with a minimum length of 43 characters
    /// and a maximum length of 128 characters.
    ///    
    /// ABNF for "code_verifier" is as follows.
    ///    
    /// code-verifier = 43*128unreserved
    /// unreserved = ALPHA / DIGIT / "-" / "." / "_" / "~"
    /// ALPHA = %x41-5A / %x61-7A
    /// DIGIT = % x30 - 39 
    ///    
    /// Reference: rfc-7636 https://datatracker.ietf.org/doc/html/rfc7636#section-4.1     
    ///</remarks>
    private static string GenerateCodeVerifier(uint size = 128)
    {
        if (size < 43 || size > 128)
            size = 128;

        const string unreservedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz123456789-._~";
        Random random = new Random();
        char[] highEntropyCryptograph = new char[size];

        for (int i = 0; i < highEntropyCryptograph.Length; i++)
        {
            highEntropyCryptograph[i] = unreservedCharacters[random.Next(unreservedCharacters.Length)];
        }

        return new string(highEntropyCryptograph);
    }

    /// <summary>
    /// Generates a code_challenge based on rfc-7636.
    /// </summary>
    /// <param name="codeVerifier">The code verifier.</param>
    /// <returns>A code challenge.</returns>
    /// <remarks> 
    /// plain
    ///    code_challenge = code_verifier
    ///    
    /// S256
    ///    code_challenge = BASE64URL-ENCODE(SHA256(ASCII(code_verifier)))
    ///    
    /// If the client is capable of using "S256", it MUST use "S256", as
    /// "S256" is Mandatory To Implement(MTI) on the server.Clients are
    /// permitted to use "plain" only if they cannot support "S256" for some
    /// technical reason and know via out-of-band configuration that the
    /// server supports "plain".
    /// 
    /// The plain transformation is for compatibility with existing
    /// deployments and for constrained environments that can't use the S256
    /// transformation.
    ///    
    /// ABNF for "code_challenge" is as follows.
    ///    
    /// code-challenge = 43 * 128unreserved
    /// unreserved = ALPHA / DIGIT / "-" / "." / "_" / "~"
    /// ALPHA = % x41 - 5A / %x61-7A
    /// DIGIT = % x30 - 39
    /// 
    /// Reference: rfc-7636 https://datatracker.ietf.org/doc/html/rfc7636#section-4.2
    /// </remarks>
    private static string GenerateCodeChallenge(string codeVerifier)
    {
        using (var sha256 = SHA256.Create())
        {
            var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            return Base64UrlEncoder.Encode(challengeBytes);
        }
    }
}

public struct OAuth2Response
{
	public string access_token { get; set; }
	public string token_type   { get; set; }
	public int    expires_in   { get; set; }
	public string scope        { get; set; }
	public string uid          { get; set; }
	public string account_id   { get; set; }
}

using System.Net;
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
		public static string[] Scope     = new string[]{"onedrive.readwrite"};
		public static string   ReturnURL = "";

		public static void GetAuth()
		{
			var Auth = new MsaAuthenticationProvider(AppID, ReturnURL, Scope);
		}
	}
	
	//High props to Dropbox. Their docs on the API is absolute top notch, unlike a certain singular drive.
	public static class DropBox
	{
		public static          string AccessToken = "";
		public static readonly string ClientID      = "gqvtt2vmwwox4tk";
		//TODO: smthn about ensuring this port is open.
		public static readonly string Loopback = "http://127.0.0.1:52475/";
		
		public static readonly Uri RedirectURI = new Uri(Loopback + "authorize");
		public static readonly Uri JSRedirectURI = new Uri(Loopback + "token");

		public static readonly string AuthURL  = "www.dropbox.com/oauth2/authorize";
		public static readonly string TokenURL = "https://api.dropboxapi.com/oauth2/token";

		public static void Sync(string? force = null)
		{
			if (force == null)
				force = "";
			string tokenResponseJson = Authenticate(AuthURL, TokenURL, ClientID);
			OAuth2Response? tokenResponse = JsonConvert.DeserializeObject<OAuth2Response>(tokenResponseJson);
			//hope tokenresponse isnt null
			AccessToken = tokenResponse.access_token;
			using (var dbx = new DropboxClient(AccessToken))
			{
				var list = dbx.Files.ListFolderAsync(string.Empty).Result;
				Metadata? todoFile = list.Entries.FirstOrDefault(i => i.Name == "todo.txt"); //Server todo.txt
				FileInfo todoFileInfo = new FileInfo(YAML.TODO_FILE); //Client todo.txt
				
				//TODO: PLEASE don't do this. Try to figure out a logging and desync fixer system.
				//	I'm pretty sure any newer device will upload because the freshly made todo file is considered newer
				
				//If the server version is OLDER than ours
				if ((todoFile == null || DateTime.Compare(todoFile.AsFile.ServerModified, todoFileInfo.LastWriteTime) < 0 || force == "upload") && force != "download")
				{
					//Upload
					Console.WriteLine("Uploading to server!");
					using (var mem = new MemoryStream(File.ReadAllBytes(YAML.TODO_FILE)))
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
						var downloadResult = response.Result.GetContentAsByteArrayAsync().Result;
						File.WriteAllBytes(YAML.TODO_FILE, downloadResult);
					}
				}
			}
		}
	}
	
	//TODO: support reauthorizing token.
	public static string Authenticate(string authUrl, string tokenUrl, string clientId)
	{
		var pkce = new Pkce();
		string URL =
			"https://" + authUrl + "?client_id=" + clientId + "&response_type=code&code_challenge=" + pkce.CodeChallenge + "&code_challenge_method=S256";
		//TODO: Launch browser automatically and get response automatically.
		Console.WriteLine("Please insert the following into your URL: " + URL + "\nAnd insert back the code: ");
		var AuthToken = Console.ReadLine();
		//TODO: Make this static.
		HttpClient client = new HttpClient();
		var postreq = new Dictionary<string, string>()
		{
			{ "code", AuthToken },
			{ "grant_type", "authorization_code" },
			{ "code_verifier", pkce.CodeVerifier },
			{ "client_id", clientId },
		};
		var content = new FormUrlEncodedContent(postreq);
		var response = client.PostAsync(tokenUrl, content).Result;
		string responseString = response.Content.ReadAsStringAsync().Result;
		return responseString;
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
    public Pkce(uint size = 128) => NewCode();

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

public class OAuth2Response
{
	public string access_token { get; set; }
	public string token_type   { get; set; }
	public int    expires_in   { get; set; }
	public string scope        { get; set; }
	public string uid          { get; set; }
	public string account_id   { get; set; }
}

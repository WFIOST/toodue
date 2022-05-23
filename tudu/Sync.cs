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
	public class OneDrive
	{
		public static string   AppID     = "24a8a6c0-f6ed-4c6f-84e3-162d411daa8a";
		public static string[] Scope     = new string[]{"onedrive.readwrite"};
		public static string   ReturnURL = "";

		public static void GetAuth()
		{
			var Auth = new MsaAuthenticationProvider(AppID, ReturnURL, Scope);
		}
	}

	public static class DropBox
	{
		public static          string AccessToken = "";
		public static readonly string ClientID      = "gqvtt2vmwwox4tk";
		//TODO: smthn about ensuring this port is open.
		public static readonly string Loopback = "http://127.0.0.1:52475/";
		
		public static readonly Uri RedirectURI = new Uri(Loopback + "authorize");
		public static readonly Uri JSRedirectURI = new Uri(Loopback + "token");

		public static void r()
		{
			using (var dbx = new DropboxClient(AccessToken))
			{
				var list = dbx.Files.ListFolderAsync(string.Empty);
				list.Wait();
				Metadata? todofile = null;
				if (list.Result.Entries.Where(i => i.Name == "todo.txt").Count() != 0)
					todofile = list.Result.Entries.First(i => i.Name == "todo.txt");
				FileInfo TodoFileInfo = new FileInfo(YAML.TODO_FILE);
				
				//TODO: DONT DO THIS JFC
				//If the server version is OLDER than ours
				if (todofile == null || DateTime.Compare(todofile.AsFile.ServerModified, TodoFileInfo.LastWriteTime) < 0)
				{
					Console.WriteLine("Uploading to server!");
					using (var mem = new MemoryStream(File.ReadAllBytes(YAML.TODO_FILE)))
					{
						var upload = dbx.Files.UploadAsync("/todo.txt", WriteMode.Overwrite.Instance, body: mem);
						upload.Wait();
					}
				}
				else //If the sever version is NEWER than ours
				{
					Console.WriteLine("Downloading from server!");
					using (var response = dbx.Files.DownloadAsync("/todo.txt"))
					{
						response.Wait();
						var f = response.Result.GetContentAsByteArrayAsync();
						f.Wait();
						var r = f.Result;
						File.WriteAllBytes(YAML.TODO_FILE, r);
					}
				}

				foreach (var item in list.Result.Entries.Where(i => i.IsFolder))
				{
					Console.WriteLine("D   {0}/", item.Name);
				}
				
				foreach (var item in list.Result.Entries.Where(i => i.IsFile))
				{
					Console.WriteLine("F{0,8} {1}/", item.AsFile.Size, item.Name);
				}
			}
		}

		public static void OAuth2()
		{
			var pkce = new Pkce();
			string URL =
				"https://www.dropbox.com/oauth2/authorize?client_id=" + ClientID + "&response_type=code&code_challenge=" + pkce.CodeChallenge + "&code_challenge_method=S256";
			Console.WriteLine("Please insert the following into your URL: " + URL + "\nAnd insert back the code: ");
			var auth = Console.ReadLine();
			
			
			//TODO: Make this static.
			HttpClient client = new HttpClient();
			var postreq =
				new Dictionary<string, string>()
				{
					{ "code", auth},
					{ "grant_type", "authorization_code"},
					{ "code_verifier", pkce.CodeVerifier },
					{ "client_id", ClientID},
				};
			var content = new FormUrlEncodedContent(postreq);
			var response = client.PostAsync("https://api.dropboxapi.com/oauth2/token", content).Result;
			var responseString = response.Content.ReadAsStringAsync().Result;
			Console.WriteLine(responseString);
			var oa2rep = JsonConvert.DeserializeObject<OAuth2Response>(responseString);
			AccessToken = oa2rep.access_token;
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
    public Pkce(uint size = 128)
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
    public static string GenerateCodeVerifier(uint size = 128)
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
    public static string GenerateCodeChallenge(string codeVerifier)
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

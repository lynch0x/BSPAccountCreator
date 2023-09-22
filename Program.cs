using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BSPCreaor
{
    class Program
    {
        
        public static HttpClient client = new HttpClient();
        static void Main()
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "dW5pdHkuY2xpZW50OnNlY3JldA==");
            new Program().MainAsync().GetAwaiter().GetResult();
            //Console.WriteLine(GetReCaptchaToken());
            //
            //Console.ReadLine();
        }
        public async Task MainAsync()
        {
           
           
            
            //Console.WriteLine(captcha);
            while (true)
            {
                Console.Clear();
                Console.WriteLine("SERVER: PL");
                Console.Write("Username: ");
                string name = Console.ReadLine();
                Console.Write("Password: ");
                string passwd = Console.ReadLine();
                string captcha = await RecaptchaBypass.GetReCaptchaToken();
                var responseMessage = await client.PostAsync("https://eu.mspapis.com/edgelogins/graphql", new StringContent(GetCreateAccountJson(name, passwd, captcha), Encoding.UTF8, "application/json"));
                if (responseMessage.IsSuccessStatusCode)
                {
                    string text = await responseMessage.Content.ReadAsStringAsync();
                    string[] sp = text.Split('"');
                    if (sp[6].Contains("true"))
                    {
                        string u = sp[15];
                        string pid = sp[19];
                        string RToken = await LoginAndGetRefreshToken(name, passwd);
                        if (RToken != null)
                        {
                            string Token = await RefreshToken(RToken, pid);
                            if (Token != null)
                            {
                                ValidateBSPAccount(Token);
                                using (Stream s = File.OpenWrite("dhneih.txt"))
                                {
                                    s.Position = s.Length;
                                    using (StreamWriter writer = new StreamWriter(s, Encoding.UTF8))
                                    {
                                        writer.WriteLine($"{u}:{passwd}:{pid}");
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Request failed!");
                }
                await Task.Delay(2000);
            }

        }
        async void ValidateBSPAccount(string Token)
        {
            await client.PostAsync("https://central-eu-alb.rbpapis.com/clusterstat/user/create", new StringContent("{\"ntk\":\"" + Token + "\"}", Encoding.ASCII, "application/json"));
        }
        async Task<string> RefreshToken(string refreshToken, string profileId)
        {
            FormUrlEncodedContent content = new FormUrlEncodedContent(new KeyValuePair<string, string>[4]
           {
                 new KeyValuePair<string, string>("client_id","unity.client"),
                new KeyValuePair<string, string>("grant_type","refresh_token"),
                new KeyValuePair<string, string>("refresh_token",refreshToken),
                new KeyValuePair<string, string>("acr_values","gameId:ywru profileId:"+profileId)
           });
            
            var response = await client.PostAsync("https://eu-secure.mspapis.com/loginidentity/connect/token", content);
            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult().Split('"')[7];
            }
            return null;
        }
        async Task<string> LoginAndGetRefreshToken(string username,string password)
        {
            FormUrlEncodedContent content = new FormUrlEncodedContent(new KeyValuePair<string, string>[5]
            {
                new KeyValuePair<string, string>("client_id","unity.client"),
                new KeyValuePair<string, string>("grant_type","password"),
                new KeyValuePair<string, string>("username","PL|"+username),
                new KeyValuePair<string, string>("password",password),
                new KeyValuePair<string, string>("acr_values","gameId:ywru")
            });
            var response = await client.PostAsync("https://eu-secure.mspapis.com/loginidentity/connect/token", content);
            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult().Split('"')[13];
            }
            return null;
        }
        string GetCreateAccountJson(string name,string password,string captcha)
        {
            return "{\"query\":\"mutation create($loginName:String!,$password:String!,$gameId:String!,$isGuest:Boolean!,$countryCode:Region!,$checksum:String!,$recaptchaV3Token:String){createLoginProfile(input:{name:$loginName,password:$password,gameId:$gameId,region:$countryCode,isGuest:$isGuest},verify:{checksum:$checksum,recaptchaV3Token:$recaptchaV3Token}){success,loginProfile{loginId,loginName,profileId,profileName,isGuest},error}}\",\"variables\":\"{\\\"checksum\\\":\\\""+ChecksumCalculator.createChecksum("ywru","PL",password,name)+"\\\",\\\"loginName\\\":\\\""+name+"\\\",\\\"password\\\":\\\""+password+ "\\\",\\\"gameId\\\":\\\"ywru\\\",\\\"isGuest\\\":false,\\\"countryCode\\\":\\\"PL\\\",\\\"recaptchaV3Token\\\":\\\""+captcha+"\\\"}\"}";

        }
    }
}

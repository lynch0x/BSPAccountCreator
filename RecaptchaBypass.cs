using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BSPCreaor
{
    class RecaptchaBypass
    {
       
        [ProtoContract]
        private class Message
        {
            [ProtoMember(2)]
            public string A;

            [ProtoMember(6)]
            public string B = "q";

            [ProtoMember(14)]
            public string C = "6LcxuOsUAAAAAI2IYDfxOvAZrwRg2T1E7sJq96eg";
        }
        public static async Task<string> GetReCaptchaToken()
        {
			string captcha = "";
			
				Message protoMessage = new Message
				{
					A = (await Program.client.GetStringAsync("https://www.google.com/recaptcha/api2/anchor?ar=1&k=6LcxuOsUAAAAAI2IYDfxOvAZrwRg2T1E7sJq96eg&co=aHR0cHM6Ly9nYW1lLmxvY2FsOjQ0Mw..&hl=en&v=sNQO7xVld1CuA2hfFHvkpVL-&size=invisible&cb=godxwmn1wdub")).Split('"')[39]
				};
				using (MemoryStream s = new MemoryStream())
				{
					Serializer.Serialize(s, protoMessage);
					ByteArrayContent content = new ByteArrayContent(s.ToArray());
					try
					{
						content.Headers.ContentType = new MediaTypeHeaderValue("application/x-protobuffer");
						HttpResponseMessage responseMessage = await Program.client.PostAsync("https://www.google.com/recaptcha/api2/reload?k=6LcxuOsUAAAAAI2IYDfxOvAZrwRg2T1E7sJq96eg", content);
						try
						{
							captcha = await responseMessage.Content.ReadAsStringAsync();
						}
						finally
						{
							((IDisposable)responseMessage)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)content)?.Dispose();
					}
				}
			
			return captcha.Split('"')[3];
		}
    }
}

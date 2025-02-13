using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace Yog.Api
{
	public class HttpRequest
	{
		private static string AUTH_TOKEN = string.Empty;
		private static readonly HttpClient _client = new HttpClient();

		public static async Task Authenticate()
		{
			try
			{
				var request = new HttpRequestMessage(HttpMethod.Post, "https://services.api.unity.com/auth/v1/token-exchange?projectId=68e1b8db-87a4-48ae-a6a9-850e344a1440&environmentId=8a031346-a532-4eb0-8175-c38444dc3215");
				request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Secret.UGS_CREDENTIALS);
				request.Content = new StringContent(JsonConvert.SerializeObject(new
				{
					scopes = new string[] { "unity.projects.get", "multiplay.allocations.create", "multiplay.allocations.list", "multiplay.allocations.get" }
				}), System.Text.Encoding.UTF8, "application/json");

				var response = await _client.SendAsync(request);
				response.EnsureSuccessStatusCode();
				AUTH_TOKEN = JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new { accessToken = string.Empty }).accessToken;
				_client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AUTH_TOKEN);
			}
			catch
			{
				throw new System.Exception("Failed to authenticate with Unity services.");
			}
		}

		public static async Task<T> Get<T>(string url)
		{
			try
			{
				if (string.IsNullOrEmpty(AUTH_TOKEN))
				{
					await Authenticate();
				}

				var request = new HttpRequestMessage(HttpMethod.Get, url);
				//request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AUTH_TOKEN);

				var response = await _client.SendAsync(request);
				if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
				{
					await Authenticate();
					//request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AUTH_TOKEN);
					response = await _client.SendAsync(request);
				}
				response.EnsureSuccessStatusCode();
				return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
			}
			catch (System.Exception e)
			{
				throw new System.Exception($"Failed to get data from {url}. {e.Message}");
			}
		}

		public static async Task Post(string url, object body)
		{
			try
			{
				if (string.IsNullOrEmpty(AUTH_TOKEN))
				{
					await Authenticate();
				}

				var request = new HttpRequestMessage(HttpMethod.Post, url);
				//request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AUTH_TOKEN);
				request.Content = new StringContent(JsonConvert.SerializeObject(body), System.Text.Encoding.UTF8, "application/json");

				var response = await _client.SendAsync(request);
				if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
				{
					await Authenticate();
					//request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AUTH_TOKEN);
					response = await _client.SendAsync(request);
				}
				response.EnsureSuccessStatusCode();
			}
			catch (System.Exception e)
			{
				throw new System.Exception($"Failed to post data to {url}. {e.Message}");
			}
		}

		public static async Task<T> Post<T>(string url, object body)
		{
			try
			{

				if (string.IsNullOrEmpty(AUTH_TOKEN))
				{
					await Authenticate();
				}

				var request = new HttpRequestMessage(HttpMethod.Post, url);
				//request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AUTH_TOKEN);
				request.Content = new StringContent(JsonConvert.SerializeObject(body), System.Text.Encoding.UTF8, "application/json");

				var response = await _client.SendAsync(request);
				if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
				{
					await Authenticate();
					//request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AUTH_TOKEN);
					response = await _client.SendAsync(request);
				}
				response.EnsureSuccessStatusCode();
				return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
			}
			catch (System.Exception e)
			{
				throw new System.Exception($"Failed to post data to {url}. {e.Message}");
			}
		}
	}
}
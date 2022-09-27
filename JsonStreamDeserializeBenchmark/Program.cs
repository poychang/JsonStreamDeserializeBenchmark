using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<DeserializeBenchmark>();

[SimpleJob(RunStrategy.ColdStart, targetCount: 10)]
[MemoryDiagnoser]
public class DeserializeBenchmark
{
    private string _jsonFileUrl;
    private const string _jsonArraryData10 = "https://blog.poychang.net/apps/json-mock-data/json-array-data-10.json";
    private const string _jsonArraryData100 = "https://blog.poychang.net/apps/json-mock-data/json-array-data-100.json";
    private const string _jsonArraryData1_000 = "https://blog.poychang.net/apps/json-mock-data/json-array-data-1000.json";
    private const string _jsonArraryData10_000 = "https://blog.poychang.net/apps/json-mock-data/json-array-data-10000.json";

    public DeserializeBenchmark()
    {
        _jsonFileUrl = _jsonArraryData10_000;
    }

    [Benchmark]
    public void NewtonsoftJson_Deserialize_From_Url()
    {
        var list = ReadFromUrl().ToList();
        Console.WriteLine($"{nameof(NewtonsoftJson_Deserialize_From_Url)} Data count: {list.Count}");

        IEnumerable<DataModel> ReadFromUrl()
        {
            var httpClient = new HttpClient();
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, _jsonFileUrl);
            using var response = httpClient.SendAsync(httpRequest).GetAwaiter().GetResult();
            var dataString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            return Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<DataModel>>(dataString)!;
        }
    }

    [Benchmark]
    public void SystemTextJson_Deserialize_From_Url()
    {
        var list = ReadFromUrl().ToList();
        Console.WriteLine($"{nameof(SystemTextJson_Deserialize_From_Url)} Data count: {list.Count}");

        IEnumerable<DataModel> ReadFromUrl()
        {
            var httpClient = new HttpClient();
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, _jsonFileUrl);
            using var response = httpClient.SendAsync(httpRequest).GetAwaiter().GetResult();
            var dataString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            return System.Text.Json.JsonSerializer.Deserialize<IEnumerable<DataModel>>(dataString)!;
        }
    }

    [Benchmark]
    public void Stream_NewtonsoftJson_Deserialize_1_From_Url()
    {
        var list = ReadFromUrlStream().ToList();
        Console.WriteLine($"{nameof(Stream_NewtonsoftJson_Deserialize_1_From_Url)} Data count: {list.Count}");

        IEnumerable<DataModel> ReadFromUrlStream()
        {
            var httpClient = new HttpClient();
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, _jsonFileUrl);
            using var response = httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();
            var dataStream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();

            return ReadJsonStream<IEnumerable<DataModel>>(dataStream);
        }

        TResult ReadJsonStream<TResult>(Stream stream)
        {
            using var reader = new StreamReader(stream);
            using var jsonReader = new Newtonsoft.Json.JsonTextReader(reader);

            return new Newtonsoft.Json.JsonSerializer().Deserialize<TResult>(jsonReader)!;
        }
    }

    [Benchmark]
    public void Stream_NewtonsoftJson_Deserialize_2_From_Url()
    {
        ReadFromUrlStream().ToList().ForEach(list =>
        {
            Console.WriteLine($"{nameof(Stream_NewtonsoftJson_Deserialize_2_From_Url)} Data count: {list.Count()}");
        });

        IEnumerable<IEnumerable<DataModel>> ReadFromUrlStream()
        {
            var httpClient = new HttpClient();
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, _jsonFileUrl);
            using var response = httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();
            var dataStream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();

            return ReadJsonStreamMultipleContent<IEnumerable<DataModel>>(dataStream).ToList();
        }

        IEnumerable<TResult> ReadJsonStreamMultipleContent<TResult>(Stream stream)
        {
            using var reader = new StreamReader(stream);
            using var jsonReader = new Newtonsoft.Json.JsonTextReader(reader);
            jsonReader.SupportMultipleContent = true;
            var serializer = new Newtonsoft.Json.JsonSerializer();

            while (jsonReader.Read())
            {
                yield return serializer.Deserialize<TResult>(jsonReader)!;
            }
        }
    }

    [Benchmark]
    public void Stream_SystemTextJson_Deserialize_From_Url()
    {
        var list = ReadFromUrlStream().ToList();
        Console.WriteLine($"{nameof(Stream_SystemTextJson_Deserialize_From_Url)} Data count: {list.Count}");

        IEnumerable<DataModel> ReadFromUrlStream()
        {
            var httpClient = new HttpClient();
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, _jsonFileUrl);
            using var response = httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();
            var dataStream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();

            return ReadJsonStreamWithSystemTextJson<IEnumerable<DataModel>>(dataStream);
        }

        TResult ReadJsonStreamWithSystemTextJson<TResult>(Stream stream)
        {
            return System.Text.Json.JsonSerializer.DeserializeAsync<TResult>(stream).GetAwaiter().GetResult()!;
        }
    }

    public class DataModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public string dob { get; set; }
        public string telephone { get; set; }
        public double score { get; set; }
        public string email { get; set; }
        public string url { get; set; }
        public string description { get; set; }
        public bool verified { get; set; }
        public int salary { get; set; }
    }
}
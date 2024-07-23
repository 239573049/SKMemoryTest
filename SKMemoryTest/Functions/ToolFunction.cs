using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.SemanticKernel;
using Newtonsoft.Json.Linq;

namespace SKMemoryTest.Functions;

public class ToolFunction
{
    /// <summary>
    /// 计算两个数的和
    /// </summary>
    /// <param name="a">第一个数</param>
    /// <param name="b">第二个数</param>
    /// <returns>和</returns>
    public int Add(int a, int b)
    {
        return a + b;
    }

    /// <summary>
    /// 判断俩个数字大小
    /// </summary>
    /// <param name="a">第一个数</param>
    /// <param name="b">第二个数</param>
    /// <returns>大的数</returns>
    [KernelFunction, System.ComponentModel.Description("判断俩个数字大小,返回大的数")]
    public double MaxDouble(double a, double b)
    {
        return a > b ? a : b;
    }

    /// <summary>
    /// 判断俩个数字大小
    /// </summary>
    /// <param name="a">第一个数</param>
    /// <param name="b">第二个数</param>
    /// <returns>大的数</returns>
    [KernelFunction, System.ComponentModel.Description("判断俩个数字大小,返回大的数")]
    public int MaxInt(int a, int b)
    {
        return a > b ? a : b;
    }

    /// <summary>
    /// 获取天气
    /// </summary>
    /// <param name="city">城市</param>
    /// <returns>天气</returns>
    /// <remarks>这是一个获取天气的方法</remarks>
    [KernelFunction, System.ComponentModel.Description("获取天气指定城市的天气")]
    public async Task<Hourly[]> GetWeather([System.ComponentModel.Description("指定的城市")] string city)
    {
        using var http = new HttpClient();

        var url =
            $"https://api.seniverse.com/v3/weather/hourly.json?key=SqskMHsGbF6Ctge2D&location={city}&language=zh-Hans&unit=c&start=0&hours=24";

        var response = await http.GetAsync(url);

        var content = await response.Content.ReadFromJsonAsync<Weather>();

        return content?.results?.First().hourly ?? [];
    }
}

public class Weather
{
    public Results[] results { get; set; }
}

public class Results
{
    public Location location { get; set; }
    public Hourly[] hourly { get; set; }
}

public class Location
{
    public string id { get; set; }
    public string name { get; set; }
    public string country { get; set; }
    public string path { get; set; }
    public string timezone { get; set; }
    public string timezone_offset { get; set; }
}

public class Hourly
{
    public string time { get; set; }
    public string text { get; set; }
    public string code { get; set; }
    public string temperature { get; set; }
    public string humidity { get; set; }
    public string wind_direction { get; set; }
    public string wind_speed { get; set; }
}
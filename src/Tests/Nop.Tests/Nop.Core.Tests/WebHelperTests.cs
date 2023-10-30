using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using NUnit.Framework;

namespace Nop.Tests.Nop.Core.Tests
{
    [TestFixture]
    public class WebHelperTests : BaseNopTest
    {
        private HttpContext _httpContext;
        private IWebHelper _webHelper;

        [OneTimeSetUp]
        public void SetUp()
        {
            _webHelper = GetService<IWebHelper>();
            _httpContext = GetService<IHttpContextAccessor>().HttpContext;

            var queryString = new QueryString(string.Empty);
            queryString = queryString.Add("Key1", "Value1");
            queryString = queryString.Add("Key2", "Value2");
            _httpContext.Request.QueryString = queryString;
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _httpContext.Request.QueryString = new QueryString(string.Empty);
        }

        [Test]
        [TestCase(false, "http")]
        [TestCase(true, "https")]
        public void CanGetStoreHost(bool useSsl, string expectedScheme)
        {
            _webHelper.GetStoreHost(useSsl).Should().Be($"{expectedScheme}://{NopTestsDefaults.HostIpAddress}/");
        }

        [Test]
        [TestCase(false, "http")]
        [TestCase(true, "https")]
        public void CanGetStoreLocation(bool useSsl, string expectedScheme)
        {
            _webHelper.GetStoreLocation(useSsl).Should().Be($"{expectedScheme}://{NopTestsDefaults.HostIpAddress}/");
        }

        [Test]
        public void CanGetStoreLocationInVirtualDirectory()
        {
            _httpContext.Request.PathBase = "/nopCommercepath";
            _webHelper.GetStoreLocation(false).Should().Be($"https://{NopTestsDefaults.HostIpAddress}/nopCommercepath/");
            _httpContext.Request.PathBase = string.Empty;
        }

        [Test]
        [TestCase("Key1", "Value1")]
        [TestCase("Key2", "Value2")]
        [TestCase("Key3", null)]
        public void CanGetQueryString(string key, string expectedValue)
        {
            _webHelper.QueryString<string>(key).Should().Be(expectedValue);
        }

        [Test]
        [TestCase(null, null, null)]
        [TestCase("https://example.com", null, "https://example.com")]
        [TestCase("https://example.com/#fragment", "param", "https://example.com/#fragment")]
        [TestCase("https://example.com/?param1=value1&param2=value1", "param1", "https://example.com/?param2=value1")]
        [TestCase("https://example.com/?param1=value1&param2=value1", "param2", "https://example.com/?param1=value1")]
        [TestCase("https://example.com/?param1=value1&param2=value1", "param3", "https://example.com/?param1=value1&param2=value1")]
        [TestCase("https://example.com/?param1=value1&param2=value1#fragment", "param1", "https://example.com/?param2=value1#fragment")]
        [TestCase("https://example.com/?param1=value1&param1=value2&param2=value1", "param1", "https://example.com/?param1=value2&param2=value1")]
        [TestCase("https://example.com/?param1=value1&param1=value2&param2=value1", "param1", "value1", "https://example.com/?param2=value1")]
        public void CanRemoveQueryString(string url, string key, string value, string expectedUrl)
        {
            _webHelper.RemoveQueryString(url, key, value).Should().Be(expectedUrl);
        }

        [Test]
        [TestCase(null, null, null, null)]
        [TestCase("https://example.com", null, null, "https://example.com")]
        [TestCase("https://example.com", "param", null, "https://example.com?param=")]
        [TestCase("https://example.com/?param1=value1&param2=value1", "param1", "value2", "https://example.com/?param1=value2&param2=value1")]
        [TestCase("https://example.com/?param1=value1&param2=value1", "param2", "value2", "https://example.com/?param1=value1&param2=value2")]
        [TestCase("https://example.com/?param1=value1&param2=value1", "param3", "value1", "https://example.com/?param1=value1&param2=value1&param3=value1")]
        [TestCase("https://example.com/?param1=value1&param2=value1", "param1", "value1", "value2", "value3", "https://example.com/?param1=value1,value2,value3&param2=value1")]
        [TestCase("https://example.com/?param1=value1&param2=value1#fragment", "param1", "value2", "https://example.com/?param1=value2&param2=value1#fragment")]
        public void CanModifyQueryString(string url, string key, string value, string newValue1, string newValue2, string expectedUrl)
        {
            _webHelper.ModifyQueryString(url, key, value, newValue1, newValue2).Should().Be(expectedUrl);
        }

        [Test]
        public void CanModifyQueryStringInVirtualDirectory()
        {
            _httpContext.Request.PathBase = "/nopCommercepath";
            _webHelper.ModifyQueryString("/nopCommercepath/Controller/Action", "param1", "value1").Should().Be("/nopCommercepath/Controller/Action?param1=value1");
            _httpContext.Request.PathBase = string.Empty;
        }
    }
}

﻿using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Leaf.xNet.Tests
{
    [TestClass]
    public class HttpRequestTests
    {
        private const string BaseUrl = "https://nghttp2.org";


        [TestMethod]
        public void UserAgentRandomizeTest()
        {
            const int generateRetries = 10;
            const int minScore = 3;
            int scores = 0;

            using (var req = new HttpRequest(BaseUrl))
            {                
                string lastUserAgent = null;
                for (int i = 0; i < generateRetries; i++)
                {
                    req.UserAgentRandomize();
                    if (string.IsNullOrEmpty(req.UserAgent))
                        Assert.Fail("Unable to generate random UserAgent");

                    if (lastUserAgent != req.UserAgent)
                        ++scores;

                    lastUserAgent = req.UserAgent;
                }
                        
                if (scores < minScore)
                    Assert.Fail($"For {generateRetries} retries unable to generate minimal count of user agents (${minScore})");

                string useragentJson = req.Get("/httpbin/user-agent").ToString();
                StringAssert.Contains(useragentJson, req.UserAgent);
            }
        }

        [TestMethod]
        public void GetTest()
        {
            const string getArgument = "getArgument";
            const string getValue = "getValue";

            using (var req = new HttpRequest(BaseUrl))
            {
                var response = req.Get("/httpbin/get?{getArgument}={getValue}");
                string source = response.ToString();

                StringAssert.Contains(source, getArgument);
                StringAssert.Contains(source, getValue);
            }
        }

        [TestMethod]
        public void PostTestFormEncoded()
        {
            const string postArgument = "postArgument";
            const string postValue = "postValue";

            using (var req = new HttpRequest(BaseUrl))
            {
                var rp = new RequestParams {
                    [postArgument] = postValue
                };

                var response = req.Post("/httpbin/post", rp);
                var source = response.ToString();

                StringAssert.Contains(source, postArgument);
                StringAssert.Contains(source, postValue);
            }
        }

        [TestMethod]
        public void PostTestMultipart()
        {
            const string postArgument = "postArgument";
            const string postValue = "postValue";

            using (var req = new HttpRequest(BaseUrl))
            {
                var rp = new MultipartContent {
                    { new StringContent(postValue), postArgument}
                };

                var response = req.Post("/httpbin/post", rp);
                var source = response.ToString();

                StringAssert.Contains(source, postArgument);
                StringAssert.Contains(source, postValue);
            }
        }

        [TestMethod]
        public void FilterCookies()
        {
            var list = new Dictionary<string, string> {
                {
                    "MS_LOGIN_COOKIE_10151=-1,R,L,null,; Expires=Sun, 30-Dec-18 16:57:13 GMT; Path=/RemovedPath",
                    "MS_LOGIN_COOKIE_10151=-1,R,L,null%2C; Expires=Sun, 30-Dec-18 16:57:13 GMT; Path=/"
                },
                {
                    "MS_LOGIN_COOKIE_10151=-1,R,L,null,; Expires=Sun, 30-Dec-18 16:57:13 GMT; Path=/",
                    "MS_LOGIN_COOKIE_10151=-1,R,L,null%2C; Expires=Sun, 30-Dec-18 16:57:13 GMT; Path=/"
                }
            };

            foreach (var item in list)
            {
                Assert.AreEqual(CookieFilters.Filter(item.Key), item.Value);
            }
        }

        [TestMethod]
        public void FilterCookieDomains()
        {
            var validDomains = new[] {
                // 1st level
                "localhost",
                // 2nd level
                "google.com",
                // 3rd level wildcard subdomains
                ".google.com",
                // 3rd level
                "maps.google.com",
                ".maps.google.com",

            };

            foreach (string validDomain in validDomains)
                Assert.IsNotNull(CookieFilters.FilterDomain(validDomain), $"Domain \"{validDomain}\" filtered bad but should be good");

            // Normalized Wildcard Domains
            //
            // 1st level with wildcard will be normalized to
            var normalizedDomains = new Dictionary<string, string> {
                {"  .localhost", "localhost"}
            };

            foreach (var normalizedDomain in normalizedDomains)
            {
                string filteredNormalizedDomain = CookieFilters.FilterDomain(normalizedDomain.Key);
                Assert.AreEqual(normalizedDomain.Value, filteredNormalizedDomain, "Invalid first level cookie domain normalization");
            }

            // Invalid Domains
            //
            var invalidDomains = new[] {
                "    ",
                null,
                "\r\n\t\t",
                "\n\n "
            };
            foreach (string invalidDomain in invalidDomains)
            {
                Assert.IsNull(CookieFilters.FilterDomain(invalidDomain), $"Domain \"{invalidDomain}\" filtered good but it should be bad");
            }
        }

        /*
        [TestMethod]
        public void KeepTempHeadersAfterRedirect()
        {
            using (var req = new HttpRequest())
            {
                req.Proxy = HttpProxyClient.Parse("127.0.0.1:8887");
                req.Proxy.AbsoluteUriInStartingLine = false;

                req.KeepTemporaryHeadersOnRedirect = true;
                req.AddHeader(HttpHeader.Referer, "https://google.com");
                var res = req.Get("http://google.com");

                var res3 = req.Get("http://ya.ru");

                req.KeepTemporaryHeadersOnRedirect = false;
                req.AddHeader(HttpHeader.Referer, "https://google.com");
                var resNo = req.Get("http://google.com");



                req.KeepTemporaryHeadersOnRedirect = true;
                req.AddHeader(HttpHeader.Referer, "https://google.com");
                req.Get("http://google.com").None();

                req.KeepTemporaryHeadersOnRedirect = false;
                req.AddHeader(HttpHeader.Referer, "https://google.com");
                req.Get("http://google.com").None();


            }
        }

        
        [TestMethod]
        public void AvoidCrossdomainCookieDuplication()
        {
            var req = new HttpRequest("https://viastyle.org");
            req.Proxy = HttpProxyClient.Parse("127.0.0.1:8888");
            CookieStorage.DefaultExpireBeforeSet = true;

            var resp = req.Get("/up/leaf-cookie.php");
            var resp2 = req.Get("/up/leaf-cookie.php?q=q");
            var resp3 = req.Get("/up/leaf-cookie.php");

        }*/

        /*
        [TestMethod]
        public void GetCookies()
        {
            using (var req = new HttpRequest())
            {
                
              string getToken = req.Get("https://www.ourtesco.com/login/").ToString();

                string token = Regex.Match(getToken, @"name=""user_info_nonce"" value=""(.+?)""").Groups[1].ToString();


                req.Referer = "https://www.ourtesco.com/login/";
                req.AddHeader("Accept-Encoding", "gzip, deflate, br");
                req.AddHeader("Accept-Language", "ru,en;q=0.9");
                req.AddHeader("Upgrade-Insecure-Requests", "1");
                req.AddHeader("Origin", "https://www.ourtesco.com");
                var postData = new RequestParams
                {
                    ["redirect_to"] = "https://www.ourtesco.com/login/",
                    // TODO: edit
                    ["log"] = "login",
                    ["pwd"] = "pass",
                    ["user_info_nonce"] = token,
                    ["_wp_http_referer"] = "/login/",
                    ["wplogin"] = "Sign in"
                };

                string postResponse = req.Post("https://www.ourtesco.com/login/", postData).ToString();
                

                // del

                var pd = new RequestParams
                {
                    ["storeId"] = "10151",
                    ["langId"] = "-24",
                    ["catalogId"] = "10051",
                    ["fromOrderId"] = "*",
                    ["toOrderId"] = ".",
                    ["deleteIfEmpty"] = "*",
                    ["createIfEmpty"] = "1",
                    ["calculationUsageId"] = "-1",
                    ["updatePrices"] = "0",
                    ["previousPage"] = "logon",
                    ["forgotPasswordURL"] = "MSResForgotPassword",
                    ["rememberMe"] = "false",
                    ["resJSON"] = "true",
                    ["reLogonURL"] = "MSResLogin",
                    ["resetConfirmationViewName"] = "MSPwdEmailConfirmModalView",
                    ["myAcctMain"] = "",
                    ["challengeAnswer"] = "-",
                    ["errorViewName"] = "MSResLogin",
                    ["continueSignIn"] = "1",
                    ["migrateUserErrorMsg"] = "MS_MIGRAT_HEADERERR_MSG",
                    ["returnPage"] = "MSUserLoyaltyOptInView",
                    ["URL"] = "/webapp/wcs/stores/servlet/MSSecureOrdercalculate?catalogId=10051&langId=-24&mergeStatus=&storeId=10151&URL=https://www.marksandspencer.com/&page=ACCOUNT_LOGIN",
                    ["orderMove"] = "/webapp/wcs/stores/servlet/OrderItemMove?calculationUsageIdentifier=MSLoginModalDisplay_orderMove&catalogId=10051&langId=-24&mergeStatus=&storeId=10151&toOrderId=.**.&URL=OrderCalculate?URL=https://www.marksandspencer.com/",
                    // todo: edit
                    ["logonId"] = "", 
                    ["logonPassword"] = ""
                };

                //req.Proxy = Socks5ProxyClient.Parse("127.0.0.1:8889");
                string rs = req.Post("https://www.marksandspencer.com/MSLogon", pd).ToString();
                //StringAssert.Contains(source, getArgument);
                //StringAssert.Contains(source, getValue);
            }
        }

        [TestMethod]
        public void PostTest2()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void PostTest3()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void PostTest4()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void PostTest5()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void PostTest6()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void PostTest7()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void PostTest8()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void PostTest9()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void PostTest10()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void PostTest11()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void PostTest12()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void PostTest13()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void RawTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void RawTest1()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void AddHeaderTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void AddXmlHttpRequestHeaderTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void AddHeaderTest1()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void CloseTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DisposeTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void ContainsCookieTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void ContainsHeaderTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void ContainsHeaderTest1()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void EnumerateHeadersTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void ClearAllHeadersTest()
        {
            Assert.Fail();
        }*/
    }
}

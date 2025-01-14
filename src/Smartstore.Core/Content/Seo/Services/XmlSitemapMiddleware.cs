﻿using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Smartstore.Core.Content.Seo
{
    public class XmlSitemapMiddleware
    {
        private readonly RequestDelegate _next;

        public XmlSitemapMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, SeoSettings seoSettings, IXmlSitemapGenerator sitemapGenerator)
        {
            var response = context.Response;

            if (!seoSettings.XmlSitemapEnabled)
            {
                response.StatusCode = 404;
                return;
            }

            try
            {
                //var index = context.Request.Query["index"].ToString().Convert<int>();
                var index = context.GetRouteValue("index")?.ToString().Convert<int>() ?? 0;
                var partition = await sitemapGenerator.GetSitemapPartAsync(index);

                using (partition.Stream)
                {
                    response.StatusCode = 200;
                    response.ContentType = "application/xml";
                    response.ContentLength = partition.Stream.Length;
                    await partition.Stream.CopyToAsync(response.Body);
                }
            }
            catch (IndexOutOfRangeException)
            {
                await SendStatus(HttpStatusCode.BadRequest, "Sitemap index is out of range.");

            }
            catch (Exception ex)
            {
                await SendStatus(HttpStatusCode.InternalServerError, ex.Message);
                //throw;
            }

            Task SendStatus(HttpStatusCode code, string message)
            {
                response.StatusCode = (int)code;
                return response.WriteAsync(message);
            }
        }
    }
}

﻿using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WilderBlog.Services;

namespace WilderBlog.Logger
{
  public class EmailLogger : ILogger
  {
    private string _categoryName;
    private Func<string, LogLevel, bool> _filter;
    private IMailService _mailService;
    private readonly IHttpContextAccessor _contextAccessor;

    public EmailLogger(string categoryName, Func<string, LogLevel, bool> filter, IMailService mailService, IHttpContextAccessor contextAccessor)
    {
      _categoryName = categoryName;
      _filter = filter;
      _mailService = mailService;
      _contextAccessor = contextAccessor;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
      // Not necessary
      return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
      return (_filter == null || _filter(_categoryName, logLevel));
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
      if (!IsEnabled(logLevel))
      {
        return;
      }

      if (formatter == null)
      {
        throw new ArgumentNullException(nameof(formatter));
      }

      var message = formatter(state, exception);

      if (string.IsNullOrEmpty(message))
      {
        return;
      }

      message = $@"Level: {logLevel}

{message}";

      if (exception != null)
      {
        message += Environment.NewLine + Environment.NewLine + exception.ToString();
      }

      var url = UriHelper.GetEncodedPathAndQuery(_contextAccessor.HttpContext.Request);
      message += Environment.NewLine + Environment.NewLine + $"Request: {url}";
      

      _mailService.SendMailAsync("logmessage.txt", "Shawn Wildermuth", "shawn@wildermuth.com", "[WilderBlog Log Message]", message).Wait();

    }
  }
}
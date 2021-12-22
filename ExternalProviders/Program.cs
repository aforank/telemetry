using Nancy;
using Nancy.Hosting.Self;
using System;

var config = new HostConfiguration { UrlReservations = { CreateAutomatically = true } };

using (var host = new NancyHost(new Uri("http://localhost:12345"), new DefaultNancyBootstrapper(), config))
{
    host.Start();
    Console.WriteLine("Running on http://localhost:12345");
    Console.ReadKey();
}
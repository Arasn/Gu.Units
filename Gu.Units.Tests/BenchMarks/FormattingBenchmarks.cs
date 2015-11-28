﻿namespace Gu.Units.Tests
{
    using System;
    using System.Diagnostics;
    using Internals.Parsing;
    using NUnit.Framework;

    // run benchmarks in release build
    [Explicit(Benchmarks.LongRunning)]
    public class FormattingBenchmarks
    {
        // 2015-11-28| speed.ToString("F1 m/s") 1 000 000 times took: 938 ms
        // 2015-11-28| $"{ speed.metresPerSecond:F1} {SpeedUnit.MetresPerSecond}" 1 000 000 times took: 765 ms
        [Test]
        public void Benchmark()
        {
            var speed = Speed.FromMetresPerSecond(1.2);
            var toString = speed.ToString("F1 m/s");
            var string_Format = $"{speed.metresPerSecond:F1} {SpeedUnit.MetresPerSecond}";

            // end warmup

            var sw = Stopwatch.StartNew();
            var n = 100000;
            for (int i = 0; i < n; i++)
            {
                toString = speed.ToString("F1 m/s");
            }

            sw.Stop();
            Console.WriteLine($"// {DateTime.Today.ToShortDateString()}| speed.ToString(\"F1 m/s\") {n:N0} times took: {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            for (int i = 0; i < n; i++)
            {
                string_Format = $"{speed.metresPerSecond:F1} {SpeedUnit.MetresPerSecond}";
            }

            sw.Stop();
            Console.WriteLine($"// {DateTime.Today.ToShortDateString()}| $\"{{ speed.metresPerSecond:F1}} {{SpeedUnit.MetresPerSecond}}\" {n:N0} times took: {sw.ElapsedMilliseconds} ms");
        }
    }
}
#!/bin/bash

# =======================================================
# Email Speed Test Script
# =======================================================

echo "⚡ Testing Email Sending Speed..."
echo "=================================="

# Test email credentials
TEST_EMAIL="yabbghagh@gmail.com"
TEST_PASSWORD="Aa12345@"

echo "📧 Test Email: $TEST_EMAIL"
echo "🔒 Using provided password"
echo ""

# Load environment variables from firstProject
cd ../firstProject
source environment.env 2>/dev/null || {
    echo "❌ Could not load firstProject environment variables"
    exit 1
}

echo "🔧 Current SMTP Configuration:"
echo "   Server: ${EmailConfiguration__SmtpServer}"
echo "   Port: ${EmailConfiguration__Port}"
echo "   SSL: ${EmailConfiguration__UseSSL}"
echo ""

# Test SMTP connectivity first
echo "🔍 Testing SMTP connectivity..."
START_TIME=$(date +%s.%N)

if timeout 5 bash -c "</dev/tcp/${EmailConfiguration__SmtpServer}/${EmailConfiguration__Port}"; then
    END_TIME=$(date +%s.%N)
    CONNECT_TIME=$(echo "$END_TIME - $START_TIME" | bc -l)
    printf "✅ SMTP connection successful (%.2f seconds)\n" $CONNECT_TIME
else
    echo "❌ SMTP connection failed"
    exit 1
fi

echo ""

# Create a simple .NET test program
echo "📝 Creating email speed test program..."

cat > ../temp_email_test.cs << 'EOF'
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: dotnet run <email> <password>");
            return;
        }

        string testEmail = args[0];
        string testPassword = args[1];

        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Create message
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Speed Test", Environment.GetEnvironmentVariable("EmailConfiguration__From")));
            message.To.Add(new MailboxAddress("", testEmail));
            message.Subject = "🚀 Email Speed Test - " + DateTime.Now.ToString("HH:mm:ss");
            message.Body = new TextPart("html")
            {
                Text = @"
                <h2>✅ Email Speed Test Successful!</h2>
                <p>This email was sent to test the SMTP configuration speed.</p>
                <p>Timestamp: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"</p>
                <p>Configuration: Port " + Environment.GetEnvironmentVariable("EmailConfiguration__Port") + @"</p>
                "
            };

            using (var client = new SmtpClient())
            {
                client.Timeout = 15000; // 15 seconds timeout
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                string smtpServer = Environment.GetEnvironmentVariable("EmailConfiguration__SmtpServer");
                int smtpPort = int.Parse(Environment.GetEnvironmentVariable("EmailConfiguration__Port"));
                string username = Environment.GetEnvironmentVariable("EmailConfiguration__Username");
                string password = Environment.GetEnvironmentVariable("EmailConfiguration__Password");
                bool useSsl = bool.Parse(Environment.GetEnvironmentVariable("EmailConfiguration__UseSSL"));

                Console.WriteLine($"🔗 Connecting to {smtpServer}:{smtpPort}...");
                var connectStart = Stopwatch.StartNew();
                
                if (useSsl && smtpPort == 465)
                {
                    await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.SslOnConnect);
                }
                else
                {
                    await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTlsWhenAvailable);
                }
                
                connectStart.Stop();
                Console.WriteLine($"✅ Connected in {connectStart.ElapsedMilliseconds}ms");

                Console.WriteLine("🔐 Authenticating...");
                var authStart = Stopwatch.StartNew();
                await client.AuthenticateAsync(username, password);
                authStart.Stop();
                Console.WriteLine($"✅ Authenticated in {authStart.ElapsedMilliseconds}ms");

                Console.WriteLine("📤 Sending email...");
                var sendStart = Stopwatch.StartNew();
                await client.SendAsync(message);
                sendStart.Stop();
                Console.WriteLine($"✅ Email sent in {sendStart.ElapsedMilliseconds}ms");

                await client.DisconnectAsync(true);
            }

            stopwatch.Stop();
            Console.WriteLine($"🎉 Total time: {stopwatch.ElapsedMilliseconds}ms ({stopwatch.Elapsed.TotalSeconds:F2} seconds)");
            
            if (stopwatch.ElapsedMilliseconds < 5000)
            {
                Console.WriteLine("🚀 EXCELLENT: Email sending is very fast!");
            }
            else if (stopwatch.ElapsedMilliseconds < 10000)
            {
                Console.WriteLine("✅ GOOD: Email sending speed is acceptable");
            }
            else
            {
                Console.WriteLine("⚠️ SLOW: Email sending is taking longer than expected");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }
}
EOF

# Create project file
cat > ../temp_email_test.csproj << 'EOF'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="MailKit" Version="4.11.0" />
    <PackageReference Include="MimeKit" Version="4.11.0" />
  </ItemGroup>
</Project>
EOF

echo "🔨 Building test program..."
cd ..
if dotnet build temp_email_test.csproj -q; then
    echo "✅ Build successful"
    echo ""
    echo "🚀 Running email speed test..."
    echo "=============================="
    dotnet run --project temp_email_test.csproj -- "$TEST_EMAIL" "$TEST_PASSWORD"
else
    echo "❌ Build failed"
    exit 1
fi

# Cleanup
echo ""
echo "🧹 Cleaning up..."
rm -f temp_email_test.cs temp_email_test.csproj
rm -rf bin obj

echo ""
echo "✅ Email speed test completed!" 
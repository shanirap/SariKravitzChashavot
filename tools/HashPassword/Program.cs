using Microsoft.AspNetCore.Identity;

if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
{
    Console.Error.WriteLine("Usage: dotnet run --project tools/HashPassword -- \"YourNewPassword\"");
    Environment.Exit(1);
}

var plain = args[0];
var hash = new PasswordHasher<UserStub>().HashPassword(new UserStub(), plain);
Console.WriteLine(hash);

internal sealed class UserStub { }

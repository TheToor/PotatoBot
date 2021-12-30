// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Microsoft.Design",
    "CA1034:Nested types should not be visible",
    Justification = "Done by design",
    Scope = "type",
    Target = "~T:PotatoBot.Modals.API.APIEndPoints"
)]

[assembly: SuppressMessage(
    "Microsoft.Performance",
    "CA1812:VersionCommand is an internal class that is apparently never instantiated.If so, remove the code from the assembly.If this class is intended to contain only static members, make it static (Shared in Visual Basic)",
    Justification = "Class will be dynamically created during runtime",
    Scope = "namespaceanddescendants",
    Target = "~N:PotatoBot.Commands"
)]

[assembly: SuppressMessage(
    "Microsoft.Globalization",
    "CA1305:Specify IFormatProvider",
    Justification = "Not required",
    Scope = "namespaceanddescendants",
    Target = "~N:PotatoBot"
)]

[assembly: SuppressMessage(
    "Microsoft.Globalization",
    "CA1307:Specify StringComparison",
    Justification = "Not required",
    Scope = "namespaceanddescendants",
    Target = "~N:PotatoBot"
)]

[assembly: SuppressMessage(
    "Microsoft.Globalization",
    "CA1304:Specify CultureInfo",
    Justification = "<Pending>",
    Scope = "namespaceanddescendants",
    Target = "~N:PotatoBot"
)]

[assembly: SuppressMessage(
    "Microsoft.Design",
    "CA1031:Do not catch general exception types",
    Justification = "Not required",
    Scope = "namespaceanddescendants",
    Target = "~N:PotatoBot"
)]

[assembly: SuppressMessage(
    "Microsoft.Reliability",
    "CA2007:Consider calling ConfigureAwait on the awaited task",
    Justification = "<Pending>",
    Scope = "namespaceanddescendants",
    Target = "~N:PotatoBot"
)]
[assembly: SuppressMessage(
    "Microsoft.Usage",
    "CA2227:Collection properties should be read only",
    Justification = "<Pending>",
    Scope = "namespaceanddescendants",
    Target = "~N:PotatoBot.Modals"
)]
[assembly: SuppressMessage(
    "Style",
    "IDE0057:Use range operator",
    Justification = "Looks ugly",
    Scope = "namespaceanddescendants",
    Target = "~N:PotatoBot"
)]
[assembly: SuppressMessage(
    "Performance",
    "CA1822:Mark members as static",
    Justification = "Not possible in Startup file",
    Scope = "namespaceanddescendants",
    Target = "~N:PotatoBot.Webhook"
)]
[assembly: SuppressMessage(
    "Style",
    "IDE0071:Simplify interpolation",
    Justification = "Don't like that style",
    Scope = "namespaceanddescendants",
    Target = "~N:PotatoBot.Webhook"
)]

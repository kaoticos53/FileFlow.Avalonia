import { dotnet } from './_framework/dotnet.js'

const is_browser = typeof window != "undefined";
if (!is_browser) throw new Error("Expected to be run in a browser");

const dotnetRuntime = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();

const config = dotnetRuntime.getConfig();

await dotnetRuntime.runMain(config.mainAssemblyName, [window.location.search]);

const splash = document.getElementById('splash');
if (splash) {
    splash.style.opacity = '0';
    setTimeout(() => splash.remove(), 500);
}

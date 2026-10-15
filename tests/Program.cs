using TinyLang.Language;

var result = TinyLanguage.Execute("let x = (2 + 4) * 7; print x; print x / 2;");
if (result.Output != string.Join(Environment.NewLine, "42", "21") || result.Variables["x"] != 42) throw new Exception("Interpreter result is incorrect.");
try { TinyLanguage.Execute("print unknown;"); throw new Exception("Unknown names must fail."); }
catch (FormatException) { }
Console.WriteLine("Tiny language checks passed.");

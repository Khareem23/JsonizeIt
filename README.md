
# 🕒 JsonizeIt

**JsonizeIt** 

JsonizeIt is a developer tool that converts your data models — written in C#, TypeScript, Java, Dart, Python, or structured formats like XML or CSV — into clean, ready-to-use JSON. Whether you're working on frontend, backend, or full-stack projects, JsonizeIt boosts productivity by eliminating the repetitive task of manually crafting JSON payloads

---
## 🤔 Technologies Used?

1. C#.Net 
2. Blazor Web Assembly
3. X-Unit
4. Bogus
5. Automapper (Optional)


---

## 🤔 Why JsonizeIt?

In modern development, JSON is the lingua franca for APIs, configuration, data exchange, and testing. Manually creating JSON from model classes or structured data is tedious and error-prone.

JsonizeIt solves this by automatically generating accurate JSON from your existing code or structured data. It helps developers:

- Quickly prototype API responses.

- Simplify test data generation.

- Improve accuracy and reduce typos.

- Save time during development and debugging.


## 📈 Benefits for Developer Productivity

- ⚡ Instant Feedback – Quickly visualize your model data as JSON.

- 🔁 Multiplatform Support – Use it on the Web, in your IDE, or the terminal.

- 🧠 Code-aware Parsing – Detects initializers and default values.

- 🔌 IDE Integration – Seamless support for your favorite tools.

- 🧪 Perfect for API Testing – Copy/paste output directly into Postman or Swagger UI.

---
## 🛠 Supported Inputs
JsonizeIt supports conversion from:

🟦 C#

🟨 TypeScript

☕ Java

💙 Dart

🐍 Python

🗂️ XML

📊 CSV

---
## 📦 Example
### ✅ Input (C#)
```csharp
public class User 
{
  public string Name { get; set; }
  public int Age { get; set; }

  public User()
  {
      Name = "Ola";
      Age = 23;
  }
}
```

### 🎯 Output (JSON)
```csharp
{
  "Name": "Ola",
  "Age": 23
}
```

## 💡 How to Use

  - **🌐 Web App (No Install Required)**
  Use JsonizeIt directly in your browser. Paste your class or data and get instant JSON output.
  🔗 Try it now www.jsonizeit.com
  &nbsp;
  - 🔌 **Visual Studio Plugin**
  Generate JSON directly from your C# models within the IDE. Just right-click and “Jsonize It”.

  - 🧩 **VS Code Extension**
  Convert JavaScript, TypeScript, Python, or Dart classes to JSON inline using the command palette or right-click context menu.

  - 💻 **.NET CLI Tool**
   JsonizeIt is also available as a .NET CLI tool:
   ```bash
    > dotnet tool install -g JsonizeIt.Cli
    > jsonizeit MyModel.cs
   ```
   Perfect for automation, scripting, or integrating into CI/CD pipelines.

---
## 🧑‍💻 Author

**Olayinka Usman Kareem**  
[GitHub](https://github.com/Khareem23) · [LinkedIn](www.linkedin.com/in/olayinka-kareem)

---

## 🤝 Contributing
We welcome contributions! Whether it's a new language parser, bug fix, or feature idea — feel free to open a PR or issue. You are contribute in the following

- Fix a bug
- Improve performance
- Add support for a new language
- Refactor or improve documentation
- Improve the user interface

Feel free to open a [Pull Request](https://github.com/Khareem23/JsonizeIt/pulls) or start a [Discussion](https://github.com/Khareem23/JsonizeIt/discussions).

Before contributing:

1. Fork the repo
2. Create your feature branch (`git checkout -b feature/your-feature`)
3. Commit your changes (`git commit -m 'Add your message'`)
4. Push your branch (`git push origin feature/your-feature`)
5. Open a pull request against develop 🚀

If you're not sure where to start, check the [issues](https://github.com/Khareem23/JsonizeIt/issues).

Let's make working with json data fun again ! ❤️

---

## 📄 License

This project is licensed under the MIT License.  
See the [LICENSE](LICENSE) file for details.

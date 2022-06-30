# Binarity
![Binarity](https://socialify.git.ci/Klrohias/Binarity/image?description=1&font=Source%20Code%20Pro&forks=1&issues=1&language=1&owner=1&pattern=Circuit%20Board&pulls=1&stargazers=1&theme=Dark)
![License](https://img.shields.io/badge/license-MIT-green?style=for-the-badge)
![NuGet](https://img.shields.io/nuget/v/Binarity?style=for-the-badge&logo=nuget)

> Serialize an object as binary data in .NET.

# 🥝 Usage
```csharp
using Binarity;
using Binarity.Atributes;

File.WriteAllBytes("./level.dat", BinaritySerialization.Serialize(
    new Level
    {
        Name = "TestLevel",
        Author = "Tester",
        Hard = 114514,
        Mobs = new Mob[]
        {
            new Mob {ShowTime = 114},
            new Mob {ShowTime = 514},
            new Mob {ShowTime = 191},
        }
    }));

var level = BinaritySerialization.Deserialize<Level>(File.ReadAllBytes("./level.dat"));
Console.WriteLine("Name: " + level.Name);
Console.WriteLine("Author: " + level.Author);
Console.WriteLine("Hard: " + level.Hard);

class Level
{
    [BinarityField("name")] public string Name { get; set; }
    [BinarityField("author")] public string Author { get; set; }
    [BinarityField("hard")] public int Hard { get; set; }
    [BinarityField("mobs")] public Mob[] Mobs { get; set; }
}

class Mob
{
    [BinarityField("show_time")] public int ShowTime { get; set; }
}
```

# License
MIT
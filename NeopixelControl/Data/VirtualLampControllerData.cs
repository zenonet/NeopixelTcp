using System.Text.Json;

namespace NeopixelControl.Data;

public class VirtualLampControllerData
{
    public int LampWidth
    {
        get => lampWidth;
        set
        {
            lampWidth = value;
            OnChange();
        }
    }

    public int LampDistance
    {
        get => lampDistance;
        set
        {
            lampDistance = value;
            OnChange();
        }
    }

    public string LampColor
    {
        get => lampColor;
        set
        {
            lampColor = value;
            OnChange();
        }
    }

    public bool AutoUpdate
    {
        get => autoUpdate;
        set
        {
            autoUpdate = value;
            OnChange();
        }
    }

    public event Action OnPropertyChange;

    public bool UseTransaction { get; set; }

    private string lampColor = "#FF0000";
    private int lampDistance = 16;
    private int lampWidth = 5;
    private bool autoUpdate = true;

    private static string dataFilePath = Path.Combine(FileSystem.AppDataDirectory, "VirtualLampControllerData.json");

    public void SaveData()
    {
        File.WriteAllTextAsync(dataFilePath, JsonSerializer.Serialize(this));
    }

    private bool loading = true;

    public static VirtualLampControllerData LoadData()
    {
        if (!File.Exists(dataFilePath))
        {
            return new();
        }

        string jsonText = File.ReadAllText(dataFilePath);
        try
        {
            VirtualLampControllerData data = JsonSerializer.Deserialize<VirtualLampControllerData>(jsonText);
            data.loading = false;
            return data;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while loading VirtualLampControllerData. Exception: {e.Message} \n Read data: {jsonText}");
            return new();
        }
    }

    private void OnChange()
    {
        if (loading) return;
        
        OnPropertyChange?.Invoke();
    }
}
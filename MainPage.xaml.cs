public partial class MainPage : ContentPage
{
    private readonly HttpClient _client = new();
    private const string BaseUrl = "https://your-api-url/api";

    public MainPage()
    {
        InitializeComponent();
    }

    private async void LoadDocuments()
    {
        if (string.IsNullOrWhiteSpace(UsernameEntry.Text))
        {
            await DisplayAlert("Error", "Please enter your username to load documents.", "OK");
            return;
        }

        var result = await _client.GetFromJsonAsync<List<Document>>($"{BaseUrl}/document/list?username={UsernameEntry.Text.Trim()}");
        DocumentList.ItemsSource = result;
    }

    private async void UploadFile(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync();
        if (result != null)
        {
            using var stream = await result.OpenReadAsync();
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(stream), "file", result.FileName);
            content.Add(new StringContent(UsernameEntry.Text), "uploadedBy");

            var response = await _client.PostAsync(BaseUrl + "/document/upload", content);
            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Success", "Uploaded", "OK");
                LoadDocuments();
            }
            else
            {
                await DisplayAlert("Error", "Upload failed", "OK");
            }
        }
    }

    private async void RegisterOrJoin(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text?.Trim();
        string accessCode = AccessCodeEntry.Text?.Trim();

        if (string.IsNullOrEmpty(username))
        {
            await DisplayAlert("Error", "Username required", "OK");
            return;
        }

        var payload = new Dictionary<string, string>
        {
            { "username", username },
            { "accessCode", accessCode ?? string.Empty }
        };

        var response = await _client.PostAsJsonAsync(BaseUrl + "/user/join", payload);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            await DisplayAlert("Success", content["message"] + "\nYour Access Code: " + content["accessCode"], "OK");
            LoadDocuments();
        }
        else
        {
            await DisplayAlert("Error", "Join failed", "OK");
        }
    }
}

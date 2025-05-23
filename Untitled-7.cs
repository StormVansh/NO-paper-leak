// MainPage.xaml.cs
public partial class MainPage : ContentPage
{
    private readonly HttpClient _client = new();
    private const string ApiUrl = "https://your-api-url/api/document";

    public MainPage()
    {
        InitializeComponent();
        LoadDocuments();
    }

    private async void LoadDocuments()
    {
        var result = await _client.GetFromJsonAsync<List<Document>>(ApiUrl + "/list");
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
            content.Add(new StringContent("testUser"), "uploadedBy");

            var response = await _client.PostAsync(ApiUrl + "/upload", content);
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
}

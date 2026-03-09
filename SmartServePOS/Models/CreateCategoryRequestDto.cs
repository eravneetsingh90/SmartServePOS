using System.Text.Json.Serialization;

namespace SmartServePOS.Models
{
    public class CreateCategoryRequestDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("displayOrder")]
        public int DisplayOrder { get; set; } = 0;
    }
}

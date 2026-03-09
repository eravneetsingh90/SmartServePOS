using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartServePOS.Models
{
    public class UpdateCategoryRequestDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("displayOrder")]
        public int DisplayOrder { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
    }
}

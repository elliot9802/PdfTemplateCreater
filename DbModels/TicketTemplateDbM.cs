using Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbModels
{
    [Table("TicketTemplate")]
    public class TicketTemplateDbM : TicketTemplate
    {
        [Required]
        public override int? ShowEventInfo { get; set; }

        [NotMapped]
        public override TicketHandling TicketsHandling
        {
            get => string.IsNullOrEmpty(TicketsHandlingJson)
                ? new TicketHandling()
                : JsonConvert.DeserializeObject<TicketHandling>(TicketsHandlingJson) ?? new TicketHandling();
            set => TicketsHandlingJson = JsonConvert.SerializeObject(value);
        }

        [Required]
        public string TicketsHandlingJson { get; set; }

        [Required]
        public override string Name { get; set; } = string.Empty;

        [Key]
        public override Guid? TicketTemplateId { get; set; }

        [Required]
        public override int? FileStorageID { get; set; }

        #region constructors

        public TicketTemplateDbM()
        {
            TicketsHandlingJson = "{}";
        }

        public TicketTemplateDbM(TemplateCUdto org) : base(org)
        {
            TicketTemplateId = org.TicketTemplateId.HasValue && org.TicketTemplateId != Guid.Empty ? org.TicketTemplateId : Guid.NewGuid();
            TicketsHandlingJson = JsonConvert.SerializeObject(org.TicketsHandling ?? new TicketHandling());
            ShowEventInfo = org.ShowEventInfo;
            Name = org.Name;
            FileStorageID = org.FileStorageID;
        }

        public void UpdateFromDTO(TemplateCUdto org)
        {
            if (org == null)
            {
                throw new ArgumentNullException(nameof(org), "Provided DTO is null.");
            }
            var ticketHandlingFromDto = !string.IsNullOrWhiteSpace(org.TicketHandlingJson)
                                        ? JsonConvert.DeserializeObject<TicketHandling>(org.TicketHandlingJson)
                                        : null;
            TicketsHandling = ticketHandlingFromDto ?? new TicketHandling();
            Name = org.Name;
            FileStorageID = org.FileStorageID;
        }

        #endregion constructors
    }
}
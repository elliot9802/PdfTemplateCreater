using Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbModels
{
    [Table("TicketTemplate")]
    public class TicketTemplateDbM : TicketTemplate
    {
        [Key]
        public override Guid TicketTemplateId { get; set; }

        [Required]
        public string TicketsHandlingJson { get; set; }

        [NotMapped]
        public override TicketHandling TicketsHandling
        {
            get => string.IsNullOrEmpty(TicketsHandlingJson) 
                ? null 
                : JsonConvert.DeserializeObject<TicketHandling>(TicketsHandlingJson);
            set => TicketsHandlingJson = JsonConvert.SerializeObject(value);
        }

        [Required]
        public override int ShowEventInfo { get; set; }

        #region constructors
        public TicketTemplateDbM() : base() 
        {
            TicketsHandlingJson = "{}";
        }

        public TicketTemplateDbM(TemplateCUdto org) : base(org)
        {
            TicketTemplateId = org.TicketTemplateId != Guid.Empty ? org.TicketTemplateId : Guid.NewGuid();
            TicketsHandlingJson = JsonConvert.SerializeObject(org.TicketsHandling ?? new TicketHandling());
            ShowEventInfo = org.ShowEventInfo;
        }

        // UpdateFromDTO could be used to update an existing TicketTemplateDbM entity
        public void UpdateFromDTO(TemplateCUdto org)
        {
            if (org == null)
            {
                throw new ArgumentNullException(nameof(org), "Provided DTO is null.");
            }

            TicketsHandling = org.TicketsHandling ?? new TicketHandling();
            ShowEventInfo = org.ShowEventInfo;
        }
        #endregion
    }
}

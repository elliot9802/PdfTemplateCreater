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
        public override int ShowEventInfo { get; set; }

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

        [Key]
        public override Guid TicketTemplateId { get; set; }

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

        public void UpdateFromDTO(TemplateCUdto org)
        {
            if (org == null)
            {
                throw new ArgumentNullException(nameof(org), "Provided DTO is null.");
            }
            var ticketHandlingFromDto = !string.IsNullOrWhiteSpace(org.TicketHandlingJson)
                                        ? JsonConvert.DeserializeObject<TicketHandling>(org.TicketHandlingJson)
                                        : null;
            if (ticketHandlingFromDto != null)
            {
                this.TicketsHandling = ticketHandlingFromDto;
            }
            else
            {
                this.TicketsHandling = new TicketHandling();
            }

            ShowEventInfo = org.ShowEventInfo;
        }

        #endregion constructors
    }
}
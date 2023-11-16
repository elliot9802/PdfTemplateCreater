﻿using Models;
//using Models.DTO;
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

        // This column will store the JSON representation of TicketHandling.
        [Column(TypeName = "nvarchar(max)")] // or use "json" if your database provider supports it
        public string TicketsHandlingJson { get; set; }

        // This property is not mapped to the database, EF will ignore it.
        [NotMapped]
        public TicketHandling TicketsHandling
        {
            get => string.IsNullOrEmpty(TicketsHandlingJson) ? null : JsonConvert.DeserializeObject<TicketHandling>(TicketsHandlingJson);
            set => TicketsHandlingJson = JsonConvert.SerializeObject(value);
        }

        [Required]
        public override int ShowEventInfo { get; set; }

        #region constructors
        //public TicketTemplateDbM UpdateFromDTO(TemplateCUdto org)
        //{
        //    TicketsHandling = org.TicketHandling;
        //    ShowEventInfo = org.ShowEventInfo;

        //    return this;
        //}
        public TicketTemplateDbM() : base() { }


        // Constructor to create a TicketTemplateDbM from a TemplateCUdto
        public TicketTemplateDbM(TemplateCUdto org) : base(org) // Call the base constructor
        {
            TicketTemplateId = Guid.NewGuid();
            // Assuming the base constructor has already populated the fields accordingly
            TicketsHandlingJson = JsonConvert.SerializeObject(org.TicketsHandling); // Serialize the TicketHandling object to JSON
        }

        // UpdateFromDTO could be used to update an existing TicketTemplateDbM entity
        public void UpdateFromDTO(TemplateCUdto org)
        {
            TicketsHandling = org.TicketsHandling; // This will automatically update TicketsHandlingJson due to the property setter
            ShowEventInfo = org.ShowEventInfo;
        }

        #endregion
    }
}

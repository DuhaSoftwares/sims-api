﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Duha.SIMS.DomainModels.Base;
using Duha.SIMS.DomainModels.Enums;
using System.Text.Json.Serialization;

namespace Duha.SIMS.DomainModels.AppUsers.Login
{
    public abstract class LoginUserDM : SIMSDomainModelBase<int>
    {
        public LoginUserDM()
        {
            //this.ProfilePicturePath = "Content/loginusers/profile/default_original.jpg";
        }

        [NotNull]
        [Required]
        public RoleTypeDM RoleType { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string LoginId { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        [DefaultValue("")]
        public string FirstName { get; set; }

        [StringLength(50, MinimumLength = 0)]
        public string? MiddleName { get; set; }
        [StringLength(50)]
        public string? LastName { get; set; }
        [MaxLength(50)]
        [EmailAddress]
        public string EmailId { get; set; }
        [StringLength(255, MinimumLength = 0)]
        [DataType(DataType.Password)]
        [Required]
        //[JsonIgnore]
        public string PasswordHash { get; set; }
        [DataType(DataType.PhoneNumber)]
        [DefaultValue(null)]

        public string? PhoneNumber { get; set; }
        public string? ProfilePicturePath { get; set; }

        [DefaultValue(false)]
        public bool IsEmailConfirmed { get; set; }
        [DefaultValue(false)]
        public bool IsPhoneNumberConfirmed { get; set; }
        public LoginStatusDM LoginStatus { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? DateOfBirth { get; set; }
    }
}

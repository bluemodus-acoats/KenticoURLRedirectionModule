using System;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using URLRedirection;

[assembly: RegisterObjectType(typeof(RedirectionTableInfo), RedirectionTableInfo.OBJECT_TYPE)]
    
namespace URLRedirection
{
    /// <summary>
    /// Data container class for <see cref="RedirectionTableInfo"/>.
    /// </summary>
	[Serializable]
    public partial class RedirectionTableInfo : AbstractInfo<RedirectionTableInfo>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "urlredirection.redirectiontable";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(RedirectionTableInfoProvider), OBJECT_TYPE, "URLRedirection.RedirectionTable", "RedirectionTableID", null, null, null, null, null, "RedirectionSiteID", null, null)
        {
			ModuleName = "URLRedirection",
			TouchCacheDependencies = true,
        };


		/// <summary>
        /// Redirection table ID.
        /// </summary>
		[DatabaseField]
        public virtual int RedirectionTableID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("RedirectionTableID"), 0);
            }
            set
            {
                SetValue("RedirectionTableID", value);
            }
        }


		/// <summary>
        /// Redirection original URL.
        /// </summary>
		[DatabaseField]
        public virtual string RedirectionOriginalURL
        {
            get
            {
                return ValidationHelper.GetString(GetValue("RedirectionOriginalURL"), String.Empty);
            }
            set
            {
                SetValue("RedirectionOriginalURL", value);
            }
        }


		/// <summary>
        /// Redirection target URL.
        /// </summary>
		[DatabaseField]
        public virtual string RedirectionTargetURL
        {
            get
            {
                return ValidationHelper.GetString(GetValue("RedirectionTargetURL"), String.Empty);
            }
            set
            {
                SetValue("RedirectionTargetURL", value);
            }
        }


		/// <summary>
        /// Redirection description.
        /// </summary>
		[DatabaseField]
        public virtual string RedirectionDescription
        {
            get
            {
                return ValidationHelper.GetString(GetValue("RedirectionDescription"), String.Empty);
            }
            set
            {
                SetValue("RedirectionDescription", value);
            }
        }


		/// <summary>
        /// Redirection site ID.
        /// </summary>
		[DatabaseField]
        public virtual int RedirectionSiteID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("RedirectionSiteID"), 0);
            }
            set
            {
                SetValue("RedirectionSiteID", value);
            }
        }


		/// <summary>
        /// Redirection type.
        /// </summary>
		[DatabaseField]
        public virtual string RedirectionType
        {
            get
            {
                return ValidationHelper.GetString(GetValue("RedirectionType"), String.Empty);
            }
            set
            {
                SetValue("RedirectionType", value);
            }
        }

        /// <summary>
        /// Redirection cultures
        /// </summary>
        [DatabaseField]
        public virtual string RedirectionCultures
        {
            get
            {
                return ValidationHelper.GetString(GetValue("RedirectionCultures"), String.Empty);
            }
            set
            {
                SetValue("RedirectionCultures", value, String.Empty);
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            RedirectionTableInfoProvider.DeleteRedirectionTableInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            RedirectionTableInfoProvider.SetRedirectionTableInfo(this);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected RedirectionTableInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="RedirectionTableInfo"/> class.
        /// </summary>
        public RedirectionTableInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="RedirectionTableInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public RedirectionTableInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}
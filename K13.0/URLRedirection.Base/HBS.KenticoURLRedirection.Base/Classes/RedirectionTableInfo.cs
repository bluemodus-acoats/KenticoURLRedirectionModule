using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

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
    public partial class RedirectionTableInfo : AbstractInfo<RedirectionTableInfo, IRedirectionTableInfoProvider>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "urlredirection.redirectiontable";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(RedirectionTableInfoProvider), OBJECT_TYPE, "URLRedirection.RedirectionTable", "RedirectionTableID", "RedirectionTableLastModified", "RedirectionTableGuid", null, null, null, "RedirectionSiteID", null, null)
        {
            ModuleName = "URLRedirection",
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("RedirectionSiteID", "cms.site", ObjectDependencyEnum.Required),
            },
            ImportExportSettings =
            {
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    // Adds the custom class into a new category in the Global objects section of the export tree
                    new ObjectTreeLocation(GLOBAL, "Redirects"),
                },
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    // Adds the custom class into a new category in the Global objects section of the staging tree
                    new ObjectTreeLocation(GLOBAL, "Redirects")
                },
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            EnabledColumn = "RedirectionEnabled"
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
        /// If unchecked, will not be used.
        /// </summary>
        [DatabaseField]
        public virtual bool RedirectionEnabled
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("RedirectionEnabled"), true);
            }
            set
            {
                SetValue("RedirectionEnabled", value);
            }
        }


        /// <summary>
        /// The URL that the incoming request should match.  Recommended using relative urls like ~/MyPage.
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
        /// If true, the Original Url must match exactly, including the query strings..
        /// </summary>
        [DatabaseField]
        public virtual bool RedirectionExactMatch
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("RedirectionExactMatch"), false);
            }
            set
            {
                SetValue("RedirectionExactMatch", value);
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
        /// If true, any query string parameters the user hits the Origin URL with will be added to the Target URL.
        /// </summary>
        [DatabaseField]
        public virtual bool RedirectionAppendQueryString
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("RedirectionAppendQueryString"), true);
            }
            set
            {
                SetValue("RedirectionAppendQueryString", value);
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
                SetValue("RedirectionDescription", value, String.Empty);
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
                return ValidationHelper.GetString(GetValue("RedirectionType"), "301");
            }
            set
            {
                SetValue("RedirectionType", value);
            }
        }


        /// <summary>
        /// This redirect is applicable for these cultures.
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
        /// If set, this redirection will override the user's culture during the redirect..
        /// </summary>
        [DatabaseField]
        public virtual string RedirectionCultureOverride
        {
            get
            {
                return ValidationHelper.GetString(GetValue("RedirectionCultureOverride"), String.Empty);
            }
            set
            {
                SetValue("RedirectionCultureOverride", value, String.Empty);
            }
        }


        /// <summary>
        /// Redirection table guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid RedirectionTableGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("RedirectionTableGuid"), Guid.Empty);
            }
            set
            {
                SetValue("RedirectionTableGuid", value);
            }
        }


        /// <summary>
        /// Redirection table last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime RedirectionTableLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("RedirectionTableLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("RedirectionTableLastModified", value);
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            Provider.Delete(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            Provider.Set(this);
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
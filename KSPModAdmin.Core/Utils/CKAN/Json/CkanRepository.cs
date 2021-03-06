﻿// <auto-generated/>
using System;
using System.Diagnostics.CodeAnalysis;

namespace KSPMODAdmin.Core.Utils.Ckan
{
    /// <summary>
    /// Class that contains the CKAN Repository infos.
    /// </summary>
    public class CkanRepository
    {
        public string name = string.Empty;
        public Uri uri;

        public override string ToString()
        {
            return String.Format("{0} ({1})", name, uri);
        }

        public static CkanRepository GitHubRepository
        {
            get { return new CkanRepository() { name = "GitHub", uri = CkanRepoManager.DefaultRepoURL }; }
        }
    }

    /// <summary>
    /// Class that contains all available CKAN Repositories.
    /// </summary>
    public struct CkanRepositories
    {
        public CkanRepository[] repositories;
    }
}

/* Copyright (c) 2021 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.IO;

namespace Gibbed.Dunia.FileFormats
{
    public static class ProjectHelpers
    {
        public static ProjectData.Project LoadProject(string projectName = null)
        {
            var basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            basePath = basePath != null ? Path.Combine(basePath, "projects") : "projects";

            if (System.IO.Directory.Exists(basePath) == false)
            {
                return null;
            }

            if (string.IsNullOrEmpty(projectName) == false)
            {
                var projectBase = Path.Combine(basePath, projectName.Trim());
                foreach (var ext in new[] { ".json", ".xml" })
                {
                    var projectPath = projectBase + ext;
                    if (System.IO.File.Exists(projectPath) == true)
                    {
                        return ProjectData.Project.Load(projectPath);
                    }
                }
                return null;
            }

            var currentPath = Path.Combine(basePath, "current.txt");
            if (System.IO.File.Exists(currentPath) == true)
            {
                var name = System.IO.File.ReadAllText(currentPath).Trim();
                var projectBase = Path.Combine(basePath, name);
                foreach (var ext in new[] { ".json", ".xml" })
                {
                    var projectPath = projectBase + ext;
                    if (System.IO.File.Exists(projectPath) == true)
                    {
                        return ProjectData.Project.Load(projectPath);
                    }
                }
            }

            return null;
        }

        public static string Modifier(string s)
        {
            return s.Replace(@"/", @"\").ToLowerInvariant();
        }

        public static void LoadListsFileNames<T>(
            this ProjectData.Project project,
            Func<string, T> hasher,
            out ProjectData.HashList<T> hashList)
        {
            hashList = project.LoadLists("*.filelist", hasher, Modifier);
        }
    }
}

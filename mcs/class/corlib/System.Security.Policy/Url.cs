//
// System.Security.Policy.Url.cs
//
// Author
//	Duncan Mak (duncan@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
// (C) 2004 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Globalization;
using System.Security.Permissions;

using Mono.Security;

namespace System.Security.Policy {

        [Serializable]
        public sealed class Url: IIdentityPermissionFactory, IBuiltInEvidence {

                private string origin_url;
                
                public Url (string name)
			: this (name, false)
                {
                }

		internal Url (string name, bool validated) 
		{
			origin_url = validated ? name : Prepare (name);
		}

		// methods

                public object Copy ()
                {
			// dont re-validate the Url
                        return new Url (origin_url, true);
                }

                public IPermission CreateIdentityPermission (Evidence evidence)
                {
                        return new UrlIdentityPermission (origin_url);
                }

                public override bool Equals (object o)
                {
			if (o is System.Security.Policy.Url)
				return (String.Compare (((Url) o).Value, Value, true, CultureInfo.InvariantCulture) == 0);
			return false;
                }

                public override int GetHashCode ()
                {
                        return origin_url.GetHashCode ();
                }

                public override string ToString ()
                {
			SecurityElement element = new SecurityElement ("System.Security.Policy.Url");
			element.AddAttribute ("version", "1");
			element.AddChild (new SecurityElement ("Url", origin_url));
			return element.ToString ();
                }

                public string Value {
			get { return origin_url; }
                }

		// interface IBuiltInEvidence

		int IBuiltInEvidence.GetRequiredSize (bool verbose) 
		{
			return (verbose ? 3 : 1) + origin_url.Length;
		}

		[MonoTODO]
		int IBuiltInEvidence.InitFromBuffer (char [] buffer, int position) 
		{
			return 0;
		}

		[MonoTODO]
		int IBuiltInEvidence.OutputToBuffer (char [] buffer, int position, bool verbose) 
		{
			return 0;
		}

		// internal

		internal string Prepare (string url) 
		{
			if (url == null)
				throw new ArgumentNullException ("Url");
			if (url == String.Empty)
				throw new FormatException (Locale.GetText ("Invalid (empty) Url"));

			// is a protocol specified
			int protocolPos = url.IndexOf (Uri.SchemeDelimiter);	// '://'
			if (protocolPos > 0) {
				if (url.StartsWith ("file://")) {
					// convert file url into uppercase
					url = "file://" + url.Substring (7).ToUpperInvariant ();
				}
				else {
					// add a trailing slash if none (lonely one) is present
					if (url.LastIndexOf ("/") == protocolPos + 2)
						url += "/";
				}
			}
			else {
				// add file scheme (default) and convert url to uppercase
				url = "file://" + url.ToUpperInvariant ();
			}

			// don't escape and don't reduce (e.g. '.' and '..')
			Uri uri = new Uri (url, false, false);
			if ((uri.Host.IndexOf ('*') < 0) || (uri.Host.Length < 2)) // lone star case
				url = uri.ToString ();
			else {
				string msg = Locale.GetText ("Invalid * character in url");
				throw new ArgumentException (msg, "name");
			}

			return url;
		}
       }
}

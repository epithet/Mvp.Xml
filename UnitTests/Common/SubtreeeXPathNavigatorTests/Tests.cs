#region using

using System;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.IO;
using System.Diagnostics;

using Mvp.Xml.Common.XPath;
#if !NUNIT
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
#endif


#endregion

namespace Mvp.Xml.Tests.SubtreeeXPathNavigatorTests
{
	[TestFixture]
	public class SubtreeTests
	{
		[Ignore]
		[Test]
		public void SubtreeSpeed() 
		{
			XPathDocument xdoc = new XPathDocument(Globals.GetResource(Globals.LibraryResource));
			XPathNavigator nav = xdoc.CreateNavigator();
			XmlDocument doc = new XmlDocument();
			doc.Load(Globals.GetResource(Globals.LibraryResource));

            XslCompiledTransform xslt = new XslCompiledTransform();
			xslt.Load("../../Common/SubtreeeXPathNavigatorTests/print_root.xsl");

            Stopwatch stopWatch = new Stopwatch();

			// Warmup
			MemoryStream stmdom = new MemoryStream();
			XmlDocument wd = new XmlDocument(); 
			wd.LoadXml(doc.DocumentElement.FirstChild.OuterXml);
			xslt.Transform(wd, null, stmdom);
			MemoryStream stmxpath = new MemoryStream();
			nav.MoveToRoot();
			nav.MoveToFirstChild();
			nav.MoveToFirstChild();
			xslt.Transform(new SubtreeXPathNavigator(nav), null, stmxpath);
			nav = doc.CreateNavigator();

			int count = 10;
			float dom = 0;
			float xpath = 0;

			for (int i = 0; i < count; i++)
			{
				GC.Collect();
				System.Threading.Thread.Sleep(1000);

				stmdom = new MemoryStream();

                stopWatch.Start();

				// Create a new document for each child
				foreach (XmlNode testNode in doc.DocumentElement.ChildNodes)
				{
					XmlDocument tmpDoc = new XmlDocument(); 
					tmpDoc.LoadXml(testNode.OuterXml);

					// Transform the subset.
					xslt.Transform(tmpDoc, null, stmdom);
				}
                stopWatch.Stop();
                dom += stopWatch.ElapsedMilliseconds;
                stopWatch.Reset();
				GC.Collect();
				System.Threading.Thread.Sleep(1000);
				
				stmxpath = new MemoryStream();

				XPathExpression expr = nav.Compile("/library/book");

                stopWatch.Start();
				XPathNodeIterator books = nav.Select(expr);
				while (books.MoveNext())
				{
					xslt.Transform(new SubtreeXPathNavigator(books.Current), null, stmxpath);
				}
                stopWatch.Stop();
                xpath += stopWatch.ElapsedMilliseconds;
                stopWatch.Reset();
			}

			Console.WriteLine("XmlDocument transformation: {0}", dom / count);
			Console.WriteLine("SubtreeXPathNavigator transformation: {0}", xpath / count);

			stmdom.Position = 0;
			stmxpath.Position = 0;

			Console.WriteLine(new StreamReader(stmdom).ReadToEnd());
			Console.WriteLine(new string('*', 100));
			Console.WriteLine(new string('*', 100));
			Console.WriteLine(new StreamReader(stmxpath).ReadToEnd());
		}

		[Ignore]
		[Test]
		public void SubtreeTransform() 
		{
            XslCompiledTransform tx = new XslCompiledTransform();
			tx.Load("../../Common/SubtreeeXPathNavigatorTests/test.xsl");

			string xml = @"
	<root>
		<salutations>
			<salute>Hi there <name>kzu</name>.</salute>
			<salute>Bye there <name>vga</name>.</salute>
		</salutations>
		<other>
			Hi there without salutations.
		</other>
	</root>";

			XmlDocument dom = new XmlDocument();
			dom.LoadXml(xml);
			tx.Transform(new DebuggingXPathNavigator(dom.DocumentElement.FirstChild.CreateNavigator()), 
				null, Console.Out);

			XPathDocument doc = new XPathDocument(new StringReader(xml));
			XPathNavigator nav = doc.CreateNavigator();
			nav.MoveToRoot();
			nav.MoveToFirstChild();
			// Salutations.
			nav.MoveToFirstChild();

			tx.Transform(new DebuggingXPathNavigator(new SubtreeXPathNavigator(nav)), null, Console.Out);
		}

		[Ignore]
		[Test]
		public void TestBooks()
		{
            XslCompiledTransform xslt = new XslCompiledTransform();
			xslt.Load("../../Common/SubtreeeXPathNavigatorTests/nodecopy.xsl");
			XPathDocument doc = new XPathDocument(Globals.GetResource(Globals.LibraryResource));

			XmlDocument dom = new XmlDocument();
			dom.Load(Globals.GetResource(Globals.LibraryResource));

			xslt.Transform(dom.DocumentElement.FirstChild.CreateNavigator(), 
				null, Console.Out);

			Console.WriteLine();

			//Navigator over first child of document element
			XPathNavigator nav = doc.CreateNavigator();
			nav.MoveToRoot();
			nav.MoveToFirstChild();
			nav.MoveToFirstChild();

			xslt.Transform(new SubtreeXPathNavigator(nav), null, Console.Out);
		}

		[Ignore]
		[Test]
		public void TestRead()
		{
			XPathDocument doc = new XPathDocument(Globals.GetResource(Globals.LibraryResource));

			//Navigator over first child of document element
			XPathNavigator nav = doc.CreateNavigator();
			nav.MoveToRoot();
			nav.MoveToFirstChild();
			nav.MoveToFirstChild();			
            Console.WriteLine(nav.OuterXml);
		}
	}
}

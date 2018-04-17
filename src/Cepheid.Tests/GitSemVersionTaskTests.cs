using NUnit.Framework;

namespace Codestellation.Cepheid.Tests
{
    [TestFixture]
    public class GitSemVersionTaskTests
    {
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Should_get_version_from_git(bool dirty)
        {
            using (var folder = new TestFolder())
            {
                // given
                folder.GitInit();
                folder.CreateRandomFile();
                folder.GitAddAll();
                folder.GitCommit();
                folder.GitTag("v3.2");
                folder.CreateRandomFile();
                folder.GitAddAll();
                folder.GitCommit();

                if (dirty)
                {
                    folder.CreateRandomFile();
                    folder.GitAddAll();
                }

                // when
                var task = new GitSemVersionTask
                {
                    WorkingDirectory = folder.Path,
                    BuildEngine = new StubBuildEngine()
                };

                bool result = task.Execute();

                // than
                Assert.That(result, Is.True);
                AssertVersion(task, "3.2.1", dirty);
            }
        }

        [Test]
        public void No_git_init()
        {
            using (var folder = new TestFolder())
            {
                // when
                var task = new GitSemVersionTask
                {
                    WorkingDirectory = folder.Path,
                    BuildEngine = new StubBuildEngine()
                };

                bool result = task.Execute();

                // than
                Assert.That(result, Is.False);
            }
        }

        [Test]
        public void No_git_commits()
        {
            using (var folder = new TestFolder())
            {
                // given
                folder.GitInit();
                folder.CreateRandomFile();

                // when
                var task = new GitSemVersionTask
                {
                    WorkingDirectory = folder.Path,
                    BuildEngine = new StubBuildEngine()
                };

                bool result = task.Execute();

                // than
                Assert.That(result, Is.False);
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void No_git_tags(bool dirty)
        {
            using (var folder = new TestFolder())
            {
                // given
                folder.GitInit();
                folder.CreateRandomFile();
                folder.GitAddAll();
                folder.GitCommit();
                folder.CreateRandomFile();
                folder.GitAddAll();
                folder.GitCommit();

                if (dirty)
                {
                    folder.CreateRandomFile();
                    folder.GitAddAll();
                }

                // when
                var task = new GitSemVersionTask
                {
                    WorkingDirectory = folder.Path,
                    BuildEngine = new StubBuildEngine()
                };

                bool result = task.Execute();

                // than
                Assert.That(result, Is.True);
                AssertVersion(task, "0.0.0", dirty);
            }
        }

        private static void AssertVersion(GitSemVersionTask task, string version, bool dirty)
        {
            Assert.That(task.Standard, Is.EqualTo(version));
            AssertStandardWithDirtyVersion(task, version, dirty);
            AssertFullVersion(task, version, dirty);
        }

        private static void AssertStandardWithDirtyVersion(GitSemVersionTask task, string version, bool dirty)
        {
            string[] parts = task.StandardWithDirty.Split('-');
            if (dirty)
            {
                Assert.That(parts.Length, Is.EqualTo(2));
                Assert.That(parts[0], Is.EqualTo(version));
                Assert.That(parts[1], Is.EqualTo("dirty"));
            }
            else
            {
                Assert.That(parts.Length, Is.EqualTo(1));
                Assert.That(parts[0], Is.EqualTo(version));
            }
        }

        private static void AssertFullVersion(GitSemVersionTask task, string version, bool dirty)
        {
            string[] parts = task.Full.Split('-');
            if (dirty)
            {
                Assert.That(parts.Length, Is.EqualTo(3));
                Assert.That(parts[0], Is.EqualTo(version));
                Assert.That(parts[1], Is.Not.Empty);
                Assert.That(parts[2], Is.EqualTo("dirty"));
            }
            else
            {
                Assert.That(parts.Length, Is.EqualTo(2));
                Assert.That(parts[0], Is.EqualTo(version));
                Assert.That(parts[1], Is.Not.Empty);
            }
        }
    }
}
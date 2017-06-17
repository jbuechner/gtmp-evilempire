using gtmp.evilempire.entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.tests.db
{
    [TestClass]
    public class CrudTests : DatabaseTestBase
    {
        [TestMethod]
        public void InsertNewUser()
        {
            using (var db = DbServiceFactory())
            {
                var user = new User { Login = "abc", Password = "xyzxyz" };
                Assert.AreEqual(user, db.Insert(user));
            }
        }

        [TestMethod]
        public void InsertUserAndSelectByKey()
        {
            using (var db = DbServiceFactory())
            {
                var user = new User { Login = "12", Password = "abc" };
                db.Insert(user);
                user.Login = "32";
                db.Insert(user);
                user.Login = "11";
                db.Insert(user);
                user.Login = "asd";
                db.Insert(user);
                var selectedUser = db.Select<User, string>("12");

                Assert.AreNotSame(user, selectedUser);
                Assert.AreEqual("12", selectedUser.Login);
                Assert.AreEqual(user.Password, selectedUser.Password);
            }
        }

        [TestMethod]
        public void SelectNonExistingEntity()
        {
            using (var db = DbServiceFactory())
            {
                var user = db.Select<User, string>("0");
                Assert.IsNull(user);
            }
        }

        [TestMethod]
        public void InsertUserWithDuplicateKey()
        {
            using (var db = DbServiceFactory())
            {
                var user = new User { Login = "abc" };
                db.Insert(user);
                try
                {
                    Assert.Fail();
                    db.Insert(user);
                }
                catch
                {
                }
            }
        }

        [TestMethod]
        public void Update()
        {
            using (var db = DbServiceFactory())
            {
                var user = new User { Login = "123", Password = "vier" };
                db.Insert(user);
                var selected = db.Select<User, string>("123");

                Assert.AreEqual("123", selected.Login);
                Assert.AreEqual("vier", selected.Password);

                selected.Password = "nup";

                db.Update(selected);

                var selected2 = db.Select<User, string>("123");

                Assert.AreEqual(selected.Login, selected2.Login);
                Assert.AreEqual(selected.Password, selected2.Password);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UpdateNonExisting()
        {
            using (var db = DbServiceFactory())
            {
                var user = new User { Login = "x" };
                db.Update(user);
            }
        }

        [TestMethod]
        public void InsertOrUpdate_NewOne()
        {
            using (var db = DbServiceFactory())
            {
                var user = new User { Login = "y", Password = "1" };
                db.InsertOrUpdate(user);

                var selected = db.Select<User, string>("y");

                Assert.AreEqual(user.Login, selected.Login);
                Assert.AreEqual(user.Password, selected.Password);
            }
        }

        [TestMethod]
        public void InsertOrUpdate_Update()
        {
            using (var db = DbServiceFactory())
            {
                var user = new User { Login = "2", Password = "now" };
                var user2 = new User { Login = "2", Password = "later" };

                db.InsertOrUpdate(user);

                var selected = db.Select<User, string>("2");

                Assert.AreEqual(user.Login, selected.Login);
                Assert.AreEqual(user.Password, selected.Password);

                db.InsertOrUpdate(user2);

                selected = db.Select<User, string>("2");

                Assert.AreEqual(user2.Login, selected.Login);
                Assert.AreEqual(user2.Password, selected.Password);
            }
        }

        [TestMethod]
        public void SelectMany()
        {
            using (var db = DbServiceFactory())
            {
                db.Insert(new Character { Id = db.NextValueFor("character"), AssociatedLogin = "abc" });
                db.Insert(new Character { Id = db.NextValueFor("character"), AssociatedLogin = "abc" });
                db.Insert(new Character { Id = db.NextValueFor("character"), AssociatedLogin = "xyz" });
                db.Insert(new Character { Id = db.NextValueFor("character"), AssociatedLogin = "asd" });
                db.Insert(new Character { Id = db.NextValueFor("character"), AssociatedLogin = "abc" });
                db.Insert(new Character { Id = db.NextValueFor("character"), AssociatedLogin = "abc" });
                db.Insert(new Character { Id = db.NextValueFor("character"), AssociatedLogin = "abc" });
                db.Insert(new Character { Id = db.NextValueFor("character"), AssociatedLogin = "asd" });
                db.Insert(new Character { Id = db.NextValueFor("character"), AssociatedLogin = "523" });

                var characters = db.SelectMany<Character, string>("abc").ToList();

                Assert.AreEqual(5, characters.Count);
                HashSet<int> ids = new HashSet<int>();
                foreach (var character in characters)
                {
                    Assert.IsTrue(ids.Add(character.Id));
                    Assert.AreEqual("abc", character.AssociatedLogin);
                }
            }
        }

        [TestMethod]
        public void SelectSingleCharacterById()
        {
            using (var db = DbServiceFactory())
            {
                db.Insert(new Character { Id = db.NextValueFor("character"), AssociatedLogin = "abc" });
                db.Insert(new Character { Id = db.NextValueFor("character"), AssociatedLogin = "def" });
                db.Insert(new Character { Id = db.NextValueFor("character"), AssociatedLogin = "geg" });
                db.Insert(new Character { Id = db.NextValueFor("character"), AssociatedLogin = "asd" });

                var character = db.Select<Character, int>(ks => ks.Id, 1);
                Assert.AreEqual("def", character.AssociatedLogin);
                Assert.AreEqual(null, character.Position);

                character = db.Select<Character, int>(ks => ks.Id, 3);
                Assert.AreEqual("asd", character.AssociatedLogin);
            }
        }

        [TestMethod]
        public void SelectSingleCharacterInventoryById()
        {
            using (var db = DbServiceFactory())
            {
                var character =  db.Insert(new Character { Id = db.NextValueFor("character") });
                var insertedInventory = db.Insert(new CharacterInventory { CharacterId = character.Id });

                var characterInventory = db.Select<CharacterInventory, int>(character.Id);

                Assert.IsNotNull(characterInventory);
                Assert.AreEqual(characterInventory.CharacterId, insertedInventory.CharacterId);
            }
        }

        [TestMethod]
        public void InsertCharacterInventoryWithCashAndReselectIt()
        {
            using (var db = DbServiceFactory())
            {
                var character = db.Insert(new Character { Id = db.NextValueFor("a") });
                var inventory = new CharacterInventory { CharacterId = character.Id };

                var moneyItemDescription = new ItemDescription { Id = 0x100, Name = "A Dollar", AssociatedCurrency = Currency.Dollar, Denomination = 1 };
                var moneyItemA = new Item { Id = db.NextInt64ValueFor("item"), ItemDescriptionId = moneyItemDescription.Id, Amount = 25 };
                var moneyItemB = new Item { Id = db.NextInt64ValueFor("item"), ItemDescriptionId = moneyItemDescription.Id, Amount = 1 };
                var moneyItemC = new Item { Id = db.NextInt64ValueFor("item"), ItemDescriptionId = moneyItemDescription.Id, Amount = 5 };

                inventory.Items.Add(moneyItemA);
                inventory.Items.Add(moneyItemB);
                inventory.Items.Add(moneyItemC);

                db.Insert(inventory);

                var reselectedCharacterInventory = db.Select<CharacterInventory, int>(character.Id);

                Assert.IsNotNull(reselectedCharacterInventory);
                Assert.IsNotNull(reselectedCharacterInventory.Items);
                Assert.AreEqual(3, reselectedCharacterInventory.Items.Count);
                Assert.AreEqual(25, reselectedCharacterInventory.Items.ElementAt(0).Amount);
                Assert.AreEqual(1, reselectedCharacterInventory.Items.ElementAt(1).Amount);
                Assert.AreEqual(5, reselectedCharacterInventory.Items.ElementAt(2).Amount);

                Assert.AreEqual(moneyItemDescription.Id, reselectedCharacterInventory.Items.ElementAt(2).ItemDescriptionId);
            }
        }
    }
}

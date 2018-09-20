using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Piramida_finansowa
{

    class Program
    {
        
        static XmlReader xml1 = XmlReader.Create(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "piramida.xml"));
        static XmlReader xml2 = XmlReader.Create(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "przelewy.xml"));
        static List<User> listUsers = new List<User>();
        static List<User> newList = new List<User>();

        static void Main(string[] args)
        {
            finansePir();
        }
        private class User
        {
            public int myId { get; set; }
            public int countUserDown { get; set; }
            public int countUserDownWithout { get; set; } = 0;
            public int depth { get; set; }
            public int headId { get; set; } = 0;
            public double money { get; set; }
            public double deposit { get; set; }

            public User(int myId, int depth, int headId = 0, int countUserDownWithout = 0)
            {
                this.myId = myId;
                this.headId = headId;
                this.depth = depth;
            }
        }
        public static void finansePir() 
        {

            using (xml1) // read and write to list information from a file "uczestnik" 
            {
                while (xml1.Read())
                {
                    if (xml1.NodeType == XmlNodeType.Element)
                    {
                        if (xml1.Name == "uczestnik")
                        {
                            listUsers.Add(new User(Convert.ToInt16(xml1.GetAttribute("id")), xml1.Depth - 1));
                        }


                    }
                }
            }

            using (xml2) //read and write to list information from a file "przelew"
            {
                while (xml2.Read())
                {
                    if (xml2.NodeType == XmlNodeType.Element)
                    {
                        if (xml2.Name == "przelew")
                        {
                            int od = Convert.ToInt16(xml2.GetAttribute("od"));
                            int kwota = Convert.ToInt16(xml2.GetAttribute("kwota"));
                            var user = listUsers.Find(x => x.myId == od);
                            user.deposit += kwota;
                        }


                    }
                }
            }

            for (int i = 1; i < listUsers.Count; i++)  // with the" depth " of the participant, find all subservient to each participant. 
            {
                if (listUsers[i].depth > listUsers[i - 1].depth) //if previouse user depth is lover than this user depth then this user is child of previous
                {
                    listUsers[i].headId = listUsers[i - 1].myId;
                    listUsers[i - 1].countUserDown++;
                }
                else if (listUsers[i].depth == listUsers[i - 1].depth) //if previous user depth same as this we shoud increase amount of childrens of Parent of previous user coz they are going one by one
                {
                    listUsers[i].headId = listUsers[i - 1].headId;
                    var userX = listUsers.Find(x => x.myId == listUsers[i].headId); //find user with same myId as headId of previous user
                    userX.countUserDown++;
                }
                else
                {
                    findDepth(listUsers[i], i);
                }
            }

            for (int i = 0; i < listUsers.Count; i++)
            {
                if (listUsers[i].countUserDown == 0)
                {
                    findWithout(listUsers[i]);
                }
            }


            for (int i = 1; i < listUsers.Count; i++)
            {
                if (listUsers[i].deposit != 0)
                {
                    listForMoney(listUsers[i]);
                    sendMoney(listUsers[i]);
                    newList.Clear();
                }
            }
            Console.WriteLine("Id, Poziom, Podwładny, Prowizja");
            Console.WriteLine();
            for (int i = 0; i < listUsers.Count; i++)
            {
                Console.WriteLine(listUsers[i].myId + ", " + listUsers[i].depth + ", " + listUsers[i].countUserDownWithout + ", " + listUsers[i].money);
            }

            Console.ReadKey();
        }

        static void sendMoney(User user) // find the profit of each
        {
            double depositUser = user.deposit;
            newList.Reverse();
            for (int i = 0; i < newList.Count; i++)
            {

                newList[i].money += Math.Floor((depositUser / 2));
                depositUser -= Math.Floor((depositUser / 2));
                if (i == newList.Count - 1) newList[i].money += depositUser;
            }

        }
        static void listForMoney(User user) // new list-tree for calculating profit
        {

            if (user.depth != 0)
            {

                var userX = listUsers.Find(x => x.myId == user.headId);
                newList.Add(userX);
                listForMoney(userX);
            }
        }
        static void findWithout(User user) // find participants who do not have their subservient in the pyramid
        {
            if (user.depth != 0)
            {
                var userX = listUsers.Find(x => x.myId == user.headId);
                userX.countUserDownWithout++;


                findWithout(userX);
            }

        }
        static void findDepth(User user, int i, int id = 0)
        {
            if (id == 0) // only at first time
            {
                if (user.depth < listUsers[i - 1].depth) // if depth of this user is lover than depth of previous than we shoud take previous user headId
                                                        //(witch represent id of it Parent element)  and compare with depth of that user myId
                {
                    id = listUsers[i - 1].headId;
                    findDepth(user, i, id);
                }
            }
            else
            {
                var userX = listUsers.Find(x => x.myId == id); //comparing current user and user that we get before (read comment upper)
                if (user.depth == userX.depth)
                {
                    user.headId = listUsers[i - 1].headId;
                    userX = listUsers.Find(x => x.myId == user.headId);
                    userX.countUserDown++;
                }
                else
                {
                    findDepth(user, i, userX.headId);
                }
            }
        }
    }

}
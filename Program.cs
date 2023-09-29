using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

using System.Xml;
using System.IO;
using Newtonsoft.Json;

namespace Pb_Lilou_Edmee_BDD
{
    class Program
    {
        static public void ReadSQL(MySqlDataReader reader)
        {
            while (reader.Read())
            {
                string currentRowAsString = "";
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string valueAsString = reader.GetValue(i).ToString();
                    currentRowAsString += valueAsString + " ";
                }
                Console.WriteLine(currentRowAsString);
            }
            Console.ReadKey();
            Console.Clear();
            reader.Close();
        }
        static public void Rapport_stat(MySqlCommand command, int choix)
        {
            MySqlDataReader reader;
            switch (choix)
            {
                case 1:
                    Console.WriteLine("Nombre de commandes:");
                    command.CommandText = "SELECT count(*) FROM commande;";
                    reader = command.ExecuteReader();
                    ReadSQL(reader);

                    Console.WriteLine("Nombre de pièces commandées:");
                    command.CommandText = "SELECT description_piece, no_catalogue,SUM(quantite_piece) FROM Contient_piece " +
                        "c JOIN piece p ON p.no_piece=c.no_piece  GROUP BY c.no_piece;";
                    reader = command.ExecuteReader();
                    ReadSQL(reader);

                    Console.WriteLine("Nombre de vélos commandés:");
                    command.CommandText = "SELECT nom_modele, SUM(c.quantite_modele) FROM Contient_modele c JOIN modele m " +
                        "ON m.no_modele=c.no_modele GROUP BY c.no_modele;";
                    reader = command.ExecuteReader();
                    ReadSQL(reader);
                    break;

                case 2:
                    Console.WriteLine("Liste des membres pour chaque programme d’adhésion:");
                    command.CommandText = "SELECT p.description_programme,GROUP_CONCAT( c.nom, pa.prenom_particulier) FROM adhere a " +
                        "JOIN client_particulier pa ON a.id_client=pa.id_client JOIN programme p ON p.no_programme=a.no_programme " +
                        "JOIN client c ON a.id_client=c.id_client GROUP BY p.no_programme;";
                    reader = command.ExecuteReader();
                    ReadSQL(reader);
                    break;

                case 3:
                    Console.WriteLine("Date d’expiration des adhésions:");
                    command.CommandText = "SELECT prenom_particulier,ADDDATE(Date_adhésion,duree) FROM Adhere a JOIN programme p " +
                        "ON a.no_programme=p.no_programme JOIN client_particulier c ON c.id_client=a.id_client ";
                    reader = command.ExecuteReader();
                    ReadSQL(reader);
                    break;
                case 4:
                    Console.WriteLine("Meilleur client par quantite de piece commandées:");
                    command.CommandText = " SELECT nom,SUM(quantite_piece) AS total FROM effectue e JOIN Client c " +
                        "ON e.id_client=c.id_client JOIN Contient_piece pi ON e.no_commande=pi.no_commande GROUP BY e.no_commande " +
                        "ORDER BY total DESC LIMIT 1; ";
                    reader = command.ExecuteReader();
                    ReadSQL(reader);

                    Console.WriteLine("Meilleur client par quantite de vélo commandés:");
                    command.CommandText = " SELECT nom, SUM(quantite_modele) AS total FROM effectue e JOIN Client c " +
                        "ON e.id_client = c.id_client JOIN Contient_modele pi ON e.no_commande = pi.no_commande GROUP BY e.no_commande " +
                        "ORDER BY total DESC LIMIT 1;";
                    reader = command.ExecuteReader();
                    ReadSQL(reader);

                    Console.WriteLine("Meilleur client par nombre de vélos et de pièces cummulés:");
                    command.CommandText = "SELECT client.nom, SUM(COALESCE(quantite_piece, 0) + COALESCE(quantite_modele, 0)) AS total " +
                        "FROM commande c LEFT JOIN contient_modele mo ON c.no_commande = mo.no_commande LEFT JOIN contient_piece pi " +
                        "ON c.no_commande = pi.no_commande JOIN effectue e ON e.no_commande = c.no_commande JOIN client " +
                        "ON client.id_client = e.id_client GROUP BY e.id_client ORDER BY total DESC;";
                    reader = command.ExecuteReader();
                    ReadSQL(reader);


                    Console.WriteLine("Meilleur client par montant commandé:");
                    command.CommandText = "SELECT client.nom, SUM(COALESCE(quantite_piece, 0)*COALESCE(prix_piece, 0)+COALESCE(quantite_modele, 0)*COALESCE(prix_modele, 0)) AS total " +
                        "FROM commande c LEFT JOIN contient_modele mo ON c.no_commande=mo.no_commande LEFT JOIN contient_piece pi " +
                        "ON c.no_commande=pi.no_commande JOIN effectue e ON e.no_commande=c.no_commande JOIN client " +
                        "ON client.id_client=e.id_client LEFT JOIN piece p ON p.no_piece=pi.no_piece LEFT JOIN modele m " +
                        "ON m.no_modele=mo.no_modele GROUP BY e.id_client ORDER BY total DESC;";
                    reader = command.ExecuteReader();
                    ReadSQL(reader);

                    break;
                case 5:
                    Console.WriteLine("Moyenne de vélo commandé par commande:");
                    command.CommandText = "SELECT AVG(quantite_modele) FROM contient_modele;";
                    reader = command.ExecuteReader();
                    ReadSQL(reader);


                    Console.WriteLine("Moyenne de pièce commandée par commande:");
                    command.CommandText = "SELECT AVG(quantite_piece) FROM contient_piece;";
                    reader = command.ExecuteReader();
                    ReadSQL(reader);

                    Console.WriteLine("Moyenne du prix des commandes:");
                    command.CommandText = "SELECT AVG(COALESCE(quantite_piece, 0)*COALESCE(prix_piece, 0) + COALESCE(quantite_modele, 0) * COALESCE(prix_modele, 0)) " +
                        "FROM commande c JOIN contient_modele mo ON mo.no_commande = c.no_commande JOIN modele m " +
                        "ON mo.no_modele = m.no_modele JOIN contient_piece pi ON pi.no_commande = c.no_commande JOIN piece p " +
                        "ON p.no_piece = pi.no_piece;";
                    reader = command.ExecuteReader();
                    ReadSQL(reader);
                    break;

            }

        }
        static void Création_client_particulier(MySqlCommand command, MySqlConnection connection)
        {
            int id = 0;
            command.Dispose();
            MySqlDataReader reader;
            command.CommandText = "SELECT MAX(id_client) FROM client;";
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                id = reader.GetInt32(0);
            }
            reader.Close();
            id++;

            Console.WriteLine("Nouveau client:\nPrénom du client:");
            string prénom = Console.ReadLine();
            Console.WriteLine("Nom client:");
            string nom = Console.ReadLine();
            Console.WriteLine("Adresse du client:");
            string adresse = Console.ReadLine();
            Console.WriteLine("Numéro de téléphone du client:");
            int tel = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Couriel du client:");
            string couriel = Console.ReadLine();
            //
            string Date = DateTime.Now.ToString("yyyy-MM-dd");

            string insertTable = " INSERT INTO client  VALUES ('" + id + "', '" + nom + "', '" + adresse + "', '" + tel + "', '" + couriel + "');";
            command.CommandText = insertTable;
            command.ExecuteNonQuery();

            insertTable = " INSERT INTO Client_particulier VALUES ('" + id + "', '" + prénom + "'); ";
            command.CommandText = insertTable;
            command.ExecuteNonQuery();

            Console.WriteLine("Voulez-vous adhérer à un programme? (true or false)");
            bool oui = Convert.ToBoolean(Console.ReadLine());
            if (oui == true)
            {
                Console.WriteLine("A quel programme souhaitez-vous adhérer?\n1: Fidélio\n2: Fidélio or\n3: Fidélio Platine \n4: Fidélio Max");
                int programme = Convert.ToInt16(Console.ReadLine());
                
                insertTable = " INSERT INTO adhere VALUES ('" + id + "', '" + Date + "', '" + programme + "'); ";
                command.CommandText = insertTable;
                command.ExecuteNonQuery();
            }
            Console.WriteLine("Le client: " + prénom + " " + nom + " a été crée. Son id client est: " + id);
            Console.ReadKey();

        }
        static void Création_client_entreprise(MySqlCommand command, MySqlConnection connection)
        {
            int id = 0;
            command.Dispose();
            MySqlDataReader reader;
            command.CommandText = "SELECT MAX(id_client) FROM client;";
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                id = reader.GetInt32(0);
            }
            reader.Close();
            id++;

            Console.WriteLine("Nouveau client:\nNom de l'entreprise:");
            string nom = Console.ReadLine();
            Console.WriteLine("Adresse de l'entreprise:");
            string adresse = Console.ReadLine();
            Console.WriteLine("Numéro de téléphone de l'entreprise:");
            int tel = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Couriel de l'entreprise:");
            string couriel = Console.ReadLine();
            
            string Date = DateTime.Now.ToString("yyyy-MM-dd");
            Console.WriteLine("Nom du contact de l'entreprise:");
            string contact = Console.ReadLine();

            string insertTable = " INSERT INTO client VALUES ('" + id + "', '" + nom + "', '" + adresse + "', '" + tel + "', '" + couriel + "');";
            command.CommandText = insertTable;
            command.ExecuteNonQuery();
            Console.WriteLine("Quelle remise souhaitez-vous avoir (en %)?");
            int remise = Convert.ToInt16(Console.ReadLine());

            insertTable = " INSERT INTO client_entreprise VALUES ('" + id + "', '" + contact + "', '" + remise + "'); ";
            command.CommandText = insertTable;
            command.ExecuteNonQuery();

            Console.WriteLine("L'entreprise: " + nom + " a bien été ajoutée comme client. Son id client est: " + id);
            Console.ReadKey();

        }
        static void Suppression_commande(MySqlCommand command, MySqlConnection connection)
        {
            Console.WriteLine("Quel est le numéro de commande que vous souhaitez supprimer?");
            MySqlDataReader reader;
            int commande = Convert.ToInt32(Console.ReadLine());
            command.CommandText = "SELECT * FROM commande WHERE no_commande=" + commande + ";";
            reader = command.ExecuteReader();
            string addresse = "";
            string commande2 = "";
            string date = "";
            while (reader.Read())
            {
                commande2 = reader.GetValue(0).ToString();
                addresse = reader.GetValue(1).ToString();
                date = reader.GetValue(2).ToString();
            }
            Console.WriteLine("Etes-vous sûr de vouloir supprimer la commande(true/false): \nn° " + commande2 + " faite le " + date + " à destination de " + addresse);
            reader.Close();
            bool oui = Convert.ToBoolean(Console.ReadLine());
            if (oui)
            {
                command.CommandText = "DELETE  FROM commande WHERE no_commande=" + commande + ";";
                reader = command.ExecuteReader();
                Console.WriteLine("La commande n° " + commande + " a bien été supprimée");

            }


        }
        static void Suppression_client(MySqlCommand command, MySqlConnection connection)
        {
            Console.WriteLine("Quel est le numéro du client que vous souhaitez supprimer?");
            MySqlDataReader reader;
            long id = Convert.ToInt32(Console.ReadLine());
            command.CommandText = "SELECT * FROM client WHERE id_client=" + id + ";";
            reader = command.ExecuteReader();
            string nom = "";
            string id2 = "";
            string addresse = "";
            while (reader.Read())
            {
                id2 = reader.GetValue(0).ToString();
                addresse = reader.GetValue(2).ToString();
                nom = reader.GetValue(1).ToString();
            }
            Console.WriteLine("Etes-vous sur de vouloir supprimer le client(true/false): \nn° " + id2 + " nommé " + nom + " dont le domicile est:  " + addresse);
            reader.Close();
            bool oui = Convert.ToBoolean(Console.ReadLine());
            if (oui)
            {
                command.CommandText = "DELETE  FROM client WHERE id_client=" + id2 + ";";
                reader = command.ExecuteReader();
                Console.WriteLine("Le client n° " + id2 + " a bien été supprimé");

            }


        }

        static void Suppression_piece(MySqlCommand command, MySqlConnection connection)
        {
            Console.WriteLine("Quel est le numéro de la piece que vous souhaitez supprimer?");
            MySqlDataReader reader;
            long no_piece = Convert.ToInt64(Console.ReadLine());
            command.CommandText = "SELECT * FROM piece WHERE no_piece=" + no_piece + ";";
            reader = command.ExecuteReader();
            string description = "";
            string no_catalogue = "";
            while (reader.Read())
            {
                no_catalogue = reader.GetValue(2).ToString();
                description = reader.GetValue(1).ToString();
            }
            Console.WriteLine("Etes-vous sûr de vouloir supprimer la pièce: \nn° " + no_piece + " qui est " + description + " reférencée dans le catalogue au numéro " + no_catalogue);
            reader.Close();
            bool oui = Convert.ToBoolean(Console.ReadLine());
            if (oui)
            {
                command.CommandText = "DELETE  FROM piece WHERE no_piece=" + no_piece + ";";
                reader = command.ExecuteReader();
                Console.WriteLine("La piece n° " + no_piece + " a bien été supprimé");

            }
        }
        static void Suppression_modele(MySqlCommand command, MySqlConnection connection)
        {
            Console.WriteLine("Quel est le numéro (de modèle) du vélo que vous souhaitez supprimer?");
            MySqlDataReader reader;
            long no_piece = Convert.ToInt64(Console.ReadLine());
            command.CommandText = "SELECT * FROM modele WHERE no_modele=" + no_piece + ";";
            reader = command.ExecuteReader();
            string nom_modele = "";
            string ligne_produit = "";
            while (reader.Read())
            {
                nom_modele = reader.GetValue(1).ToString();
                ligne_produit = reader.GetValue(6).ToString();
            }
            Console.WriteLine("Etes-vous sûr de vouloir supprimer le " + ligne_produit + " nommé " + nom_modele);
            reader.Close();
            bool oui = Convert.ToBoolean(Console.ReadLine());
            if (oui)
            {
                command.CommandText = "DELETE  FROM modele WHERE no_modele=" + no_piece + ";";
                reader = command.ExecuteReader();
                Console.WriteLine("Le vélo n° " + no_piece + " a bien été supprimé");

            }
        }
        static void Suppression_fournisseur(MySqlCommand command, MySqlConnection connection)
        {
            Console.WriteLine("Quel est le numéro de SIRET du fournisseur que vous souhaitez supprimer?");
            MySqlDataReader reader;
            long id = Convert.ToInt64(Console.ReadLine());
            command.CommandText = "SELECT * FROM fournisseur WHERE siret=" + id + ";";
            reader = command.ExecuteReader();
            string nom = "";
            while (reader.Read())
            {
                nom = reader.GetValue(1).ToString();
            }
            Console.WriteLine("Etes-vous sûr de vouloir supprimer le fournisseur dont le siret est n° " + id + " nommé " + nom);
            reader.Close();
            bool oui = Convert.ToBoolean(Console.ReadLine());
            if (oui)
            {
                command.CommandText = "DELETE  FROM fournisseur WHERE siret=" + id + ";";
                reader = command.ExecuteReader();
                Console.WriteLine("Le fournisseur  " + nom + " a bien été supprimé");

            }
        }




        static void Gestion_piece(MySqlCommand command)
        {
            MySqlDataReader reader;

            Console.WriteLine("Nombre de pieces total");
            command.CommandText = "SELECT  SUM(stock) FROM piece;";
            reader = command.ExecuteReader();
            ReadSQL(reader);

            Console.WriteLine("Stock disponible de chaque pièce");
            command.CommandText = "SELECT no_catalogue, description_piece, stock FROM piece;";
            reader = command.ExecuteReader();
            ReadSQL(reader);

            Console.WriteLine("Stock disponible de pièce par fournisseur");
            command.CommandText = "SELECT fr.nom_fournisseur, SUM(stock) FROM fournit f JOIN fournisseur fr " +
                "ON fr.siret = f.siret JOIN piece s ON s.no_piece = f.no_piece GROUP BY f.siret;";
            reader = command.ExecuteReader();
            ReadSQL(reader);

        }

        static void Gestion_modele(MySqlCommand command)
        {
            MySqlDataReader reader;

            Console.WriteLine("Nombre de modele au total");
            command.CommandText = "SELECT  SUM(stock_modele) FROM modele;";
            reader = command.ExecuteReader();
            ReadSQL(reader);

            Console.WriteLine("Stock disponible de chaque modèle");
            command.CommandText = "SELECT nom_modele, stock_modele FROM modele;";
            reader = command.ExecuteReader();
            ReadSQL(reader);
        }

        static void Maj_piece_prix(MySqlCommand command, MySqlConnection connection)
        {
            Console.WriteLine("Quel est le numéro de votre pièce?");
            int num = Convert.ToInt32(Console.ReadLine());
            MySqlDataReader reader;
            command.CommandText = "SELECT * FROM piece WHERE no_piece=" + num + ";";
            reader = command.ExecuteReader();
            string prix_piece = "";
            string description_piece = "";
            string no_catalogue = "";
            while (reader.Read())
            {
                no_catalogue = reader.GetValue(2).ToString();
                description_piece = reader.GetValue(1).ToString();
                prix_piece = reader.GetValue(3).ToString();
            }
            reader.Close();
            Console.WriteLine("Votre pièce " + description_piece + " cataloguée au numéro " + no_catalogue + " à un prix de " + prix_piece);
            Console.WriteLine("Quel nouveau prix voulez-vous donner à cette pièce?");
            int prix = Convert.ToInt32(Console.ReadLine());
            command.CommandText = "UPDATE piece SET prix_piece=" + prix + " WHERE no_piece=" + num + " ;";
            reader = command.ExecuteReader();
            reader.Close();
            Console.WriteLine("Le prix de la piece n°" + num + " est maintenant de " + prix);
        }

        static void Maj_vélo_prix(MySqlCommand command, MySqlConnection connection)
        {
            Console.WriteLine("Quel est le numéro de modèle de votre vélo?");
            string num = Console.ReadLine();
            MySqlDataReader reader;
            command.CommandText = "SELECT * FROM modele WHERE no_modele=" + num + ";";
            reader = command.ExecuteReader();
            string prix_modele = "";
            string no_modele = "";
            string ligne_produit = "";
            while (reader.Read())
            {
                ligne_produit = reader.GetValue(6).ToString();
                no_modele = reader.GetValue(0).ToString();
                prix_modele = reader.GetValue(3).ToString();
            }
            reader.Close();
            Console.WriteLine("Le modele de votre vélo est  " + no_modele + " de catégorie " + ligne_produit + " a un prix de " + prix_modele);
            Console.WriteLine("Quel nouveau prix voulez-vous donner à ce vélo?");
            int prix = Convert.ToInt32(Console.ReadLine());
            command.CommandText = "UPDATE modele SET prix_modele=" + prix + " WHERE no_modele=" + no_modele + " ;";
            reader = command.ExecuteReader();
            reader.Close();
            Console.WriteLine("Le prix du vélo " + no_modele + " est maintenant de " + prix + " euros");
        }

        static void Maj_client(MySqlCommand command, MySqlConnection connection)
        {
            Console.WriteLine("Quel est l'id de votre client?");
            int id = Convert.ToInt32(Console.ReadLine());
            MySqlDataReader reader;
            command.CommandText = "SELECT * FROM client WHERE id_client=" + id + ";";
            reader = command.ExecuteReader();
            string nom = "";
            string modifie = "";
            string description = "";
            int choix = 0;
            string valeur = "";

            string Date = DateTime.Now.ToString("yyyy-MM-dd");

            while (reader.Read())
            {
                nom = reader.GetValue(1).ToString();
                Console.WriteLine("Que souhaitez-vous modifier du client " + nom + "?\n1: Adresse \n2: Téléphone \n3: Couriel \n4: Programme d'adhésion");
                choix = Convert.ToInt32(Console.ReadLine());
                modifie = reader.GetValue(choix + 1).ToString();

            }
            switch (choix)
            {
                case 1:
                    description = "Adresse";
                    break;
                case 2:
                    description = "Téléphone";
                    break;
                case 3:
                    description = "Courriel";
                    break;
                case 4:
                    description = "Programme d'adhésion";
                    Console.WriteLine("A quel programme souhaitez-vous adhérer?\n1: Fidélio\n2: Fidélio or\n3: Fidélio Platine \n4: Fidélio Max");
                    valeur = Console.ReadLine();

                    break;
            }

            if (choix!=4)
            {
                Console.WriteLine("Quelle est la nouvelle valeur de " + description);
                valeur = Console.ReadLine();
            }
            
            reader.Close();
            switch (choix)
            {
                case 1:
                    command.CommandText = "UPDATE client SET adresse  = \'" + valeur + "\' WHERE id_CLIENT=" + id + " ;";
                    break;
                case 2:
                    command.CommandText = "UPDATE client SET telephone  = \'" + valeur + " \' WHERE id_CLIENT=" + id + " ;";
                    break;
                case 3:
                    command.CommandText = "UPDATE client SET courriel  = \'" + valeur + "\' WHERE id_CLIENT=" + id + " ;";
                    break;
                case 4:
                    command.CommandText = " INSERT INTO adhere VALUES ('" + id + "', '" + Date + "', '" + valeur + "'); ";
                    break;
            }
            reader = command.ExecuteReader();
            reader.Close();
            Console.WriteLine("Le client: " + id + " a été modifié.");
        }

        static void Maj_fournisseur(MySqlCommand command, MySqlConnection connection)
        {
            Console.WriteLine("Quel est le numéro de siret de votre fournisseur?");
            long id = Convert.ToInt64(Console.ReadLine());
            MySqlDataReader reader;
            command.CommandText = "SELECT * FROM fournisseur WHERE siret=" + id + ";";
            reader = command.ExecuteReader();
            string nom = "";
            string modifie = "";
            string description = "";
            int choix = 0;
            while (reader.Read())
            {
                nom = reader.GetValue(1).ToString();
                Console.WriteLine("Que souhaitez-vous modifier du fournisseur " + nom + "?\n1: Contact \n2: Adresse \n3: Libelle");
                choix = Convert.ToInt32(Console.ReadLine());
                modifie = reader.GetValue(choix + 1).ToString();

            }
            switch (choix)
            {
                case 1:
                    description = "Contact";
                    break;
                case 2:
                    description = "Adresse";
                    break;
                case 3:
                    description = "Libelle";
                    break;
            }

            Console.WriteLine("Quelle est la nouvelle valeur de " + description);
            string valeur = Console.ReadLine();
            reader.Close();
            Console.WriteLine(choix + description + modifie + valeur);
            switch (choix)
            {
                case 1:
                    command.CommandText = "UPDATE fournisseur SET contact_fournisseur  = \'" + valeur + "\' WHERE siret=" + id + " ;";
                    description = "adresse";
                    break;
                case 2:
                    command.CommandText = "UPDATE fournisseur SET adresse_fournisseur  = \'" + valeur + "\' WHERE siret=" + id + " ;";
                    description = "telephone";
                    break;
                case 3:
                    command.CommandText = "UPDATE fournisseur SET libelle  = \'" + valeur + "\' WHERE siret=" + id + " ;";
                    break;
            }

            reader = command.ExecuteReader();
            reader.Close();
            Console.WriteLine("Le fournisseur n° " + id + " a été modifié.");
        }


        static void Maj_Commande(MySqlCommand command, MySqlConnection connection)
        {
            Console.WriteLine("Quel est le numéro de votre commande?");
            int id = Convert.ToInt32(Console.ReadLine());
            MySqlDataReader reader;
            command.CommandText = "SELECT * FROM commande WHERE no_commande=" + id + ";";
            reader = command.ExecuteReader();

            string adresse = "";
            string date = "";
            int choix = 0;
            while (reader.Read())
            {
                adresse = reader.GetValue(1).ToString();
                date = reader.GetValue(2).ToString();
            }
            Dictionary<string, string> pieces = new Dictionary<string, string>();
            reader.Close();
            command.CommandText = "SELECT * FROM contient_piece WHERE no_commande=" + id + ";";
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                string piece = reader.GetValue(2).ToString();
                string quantite = reader.GetValue(1).ToString();
                pieces.Add(piece, quantite);
            }
            Console.WriteLine("Votre commande n°" + id + "à destination de" + adresse + "  contient: ");
            foreach (KeyValuePair<string, string> kvp in pieces)
            {
                Console.WriteLine(kvp.Value + " piece n° " + kvp.Key);
            }
            Dictionary<string, string> modeles = new Dictionary<string, string>();
            reader.Close();

            command.CommandText = "SELECT * FROM contient_modele WHERE no_commande=" + id + ";";
            reader = command.ExecuteReader();
            while (reader.Read())
            {

                string modele = reader.GetValue(2).ToString();
                string quantite2 = reader.GetValue(1).ToString();
                modeles.Add(modele, quantite2);
            }
            foreach (KeyValuePair<string, string> kvp in modeles)
            {
                Console.WriteLine(kvp.Value + " modele n° " + kvp.Key);

            }
            reader.Close();
            string valeur = "";
            Console.WriteLine("Que voulez vous faire? \n1:Modifier l'addresse 2:Supprimer une pièce 3:Supprimer un modèle 4:Ajouter une pièces 5:Ajouter un vélo");
            choix = Convert.ToInt32(Console.ReadLine());
            int quantitee = 0;
            switch (choix)
            {
                case 1:
                    Console.WriteLine("Quelle est la nouvelle adresse ? ");
                    valeur = Console.ReadLine();
                    break;
                case 2:
                    Console.WriteLine("Quel est le numéro de la pièce ? ");
                    valeur = Console.ReadLine();
                    break;
                case 3:
                    Console.WriteLine("Quelle est le numéro du modèle ? ");
                    valeur = Console.ReadLine();
                    break;
                case 4:
                    Console.WriteLine("Quelle est le numéro de la piece ? ");
                    valeur = Console.ReadLine();
                    Console.WriteLine("En quelle quantite? ");
                    quantitee = Convert.ToInt32(Console.ReadLine());
                    break;
                case 5:
                    Console.WriteLine("Quelle est le numéro du modele ? ");
                    valeur = Console.ReadLine();
                    Console.WriteLine("En quelle quantite ? ");
                    quantitee = Convert.ToInt32(Console.ReadLine());
                    break;
            }
            reader.Close();
            switch (choix)
            {
                case 1:
                    command.CommandText = "UPDATE commande SET adresse_livraison  = \'" + valeur + "\' WHERE no_commande=" + id + " ;";
                    break;
                case 2:
                    command.CommandText = "DELETE  FROM contient_piece WHERE no_commande=" + id + " AND no_piece= " + valeur + ";";
                    break;
                case 3:
                    command.CommandText = "DELETE  FROM contient_modele WHERE no_commande=" + id + " AND no_modele= " + valeur + ";";
                    break;
                case 4:
                    command.CommandText = " INSERT INTO Contient_piece VALUES ('" + id + "', '" + quantitee + "', '" + valeur + "'); ";
                    break;
                case 5:
                    command.CommandText = " INSERT INTO Contient_modele VALUES ('" + id + "', '" + quantitee + "', '" + valeur + "'); ";
                    break;
            }
            Console.WriteLine("La commande n° " + id + " a été modifiée.");
            reader = command.ExecuteReader();
            reader.Close();
        }


        static void Création_commande(MySqlCommand command, MySqlConnection connection)
        {
            int qt_restanct = 0;
            int delai = 0;
            int no_commande = 0;
            command.Dispose();
            MySqlDataReader reader;
            command.CommandText = "SELECT MAX(no_commande) FROM commande;";
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                no_commande = reader.GetInt32(0);
            }
            reader.Close();
            no_commande++;
            command.Dispose();

            Console.WriteLine("Nouvelle commande:\nId du client:");
            string id = Console.ReadLine();

            Console.WriteLine("Adresse de la commande:");
            string adresse = Console.ReadLine();
            //
            string Date = DateTime.Now.ToString("yyyy-MM-dd");
            Dictionary<string, int> pieces = new Dictionary<string, int>();
            Console.WriteLine("Voulez-vous des pièces:(true or false)");
            bool oui = Convert.ToBoolean(Console.ReadLine());
            while (oui == true)
            {
                Console.WriteLine("Quelle pièce voulez-vous?");
                string piece = Console.ReadLine();
                Console.WriteLine("Quelle quantité voulez-vous?");
                int quantite = Convert.ToInt32(Console.ReadLine());
                pieces.Add(piece, quantite);

                command.CommandText = "SELECT stock_piece,delai_approvisionnement FROM piece WHERE no_piece=" + piece + " ;";
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    qt_restanct = reader.GetInt32(0);
                    delai = reader.GetInt32(1);
                }
                reader.Close();
                int difference = qt_restanct - quantite;
                if (difference < 0)
                {
                    Console.WriteLine("Le stock de cette pièce est insuffisant, nous la commandons donc auprès de notre fournisseur. \nLe délai d'approvisionement est de " + delai + " jours");
                    difference = 10;
                }
                command.CommandText = "UPDATE piece SET stock_piece  = \'" + difference + "\' WHERE no_piece=" + piece + " ;";
                reader = command.ExecuteReader();
                reader.Close();
                Console.WriteLine("Voulez-vous d'autres pièces: (true or false)");
                oui = Convert.ToBoolean(Console.ReadLine());
            }
            Dictionary<string, int> vélos = new Dictionary<string, int>();
            Console.WriteLine("Voulez-vous des vélos: (true or false)");
            oui = Convert.ToBoolean(Console.ReadLine());
            while (oui == true)
            {
                Console.WriteLine("Quel velo voulez-vous?");
                string velo = Console.ReadLine();
                Console.WriteLine("Quelle quantité voulez-vous?");
                int quantite = Convert.ToInt32(Console.ReadLine());
                vélos.Add(velo, quantite);
                command.CommandText = "SELECT stock_modele FROM modele WHERE no_modele=" + velo + " ;";
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    qt_restanct = reader.GetInt32(0);
                }
                reader.Close();
                int difference = qt_restanct - quantite;
                if (difference < 0)
                {
                    Console.WriteLine("Le stock de ce vélo est insuffisant, nous allons donc commander les pièces nécessaires à sa construction. \n Le délai de commande sera donc plus long");
                    difference = 10;
                }
                command.CommandText = "UPDATE modele SET stock_modele  = \'" + difference + "\' WHERE no_modele=" + velo + " ;";
                reader = command.ExecuteReader();
                reader.Close();

                Console.WriteLine("Voulez-vous d'autres velos: (true or false)");
                oui = Convert.ToBoolean(Console.ReadLine());
            }
            string insertTable = " INSERT INTO commande  VALUES ('" + no_commande + "', '" + adresse + "', '" + Date + "');";
            command.CommandText = insertTable;
            try
            {
                command.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                Console.WriteLine(" ErreurConnexion : " + e.ToString());
                Console.ReadLine();
                return;
            }
            insertTable = " INSERT INTO effectue VALUES ('" + no_commande + "', '" + id + "'); ";
            command.CommandText = insertTable;
            try
            {
                command.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                Console.WriteLine(" ErreurConnexion : " + e.ToString());
                Console.ReadLine();
                return;
            }
            if (pieces != null)
            {
                foreach (KeyValuePair<string, int> kvp in pieces)
                {
                    Console.WriteLine(kvp.Value + " " + kvp.Key);
                    insertTable = " INSERT INTO Contient_piece VALUES ('" + no_commande + "', '" + kvp.Value + "', '" + kvp.Key + "'); ";
                    command.CommandText = insertTable;
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (MySqlException e)
                    {
                        Console.WriteLine(" ErreurConnexion : " + e.ToString());
                        Console.ReadLine();
                        return;
                    }
                }

            }

            if (vélos != null)
            {
                foreach (KeyValuePair<string, int> kvp in vélos)
                {
                    insertTable = " INSERT INTO Contient_modele VALUES ('" + no_commande + "', '" + kvp.Value + "', '" + kvp.Key + "'); ";
                    command.CommandText = insertTable;
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (MySqlException e)
                    {
                        Console.WriteLine(" ErreurConnexion : " + e.ToString());
                        Console.ReadLine();
                        return;
                    }
                }
            }
            Console.WriteLine("Votre commande a bien eté crée. Son numéro est:" + no_commande);
        }


        static void Création_fournisseur(MySqlCommand command, MySqlConnection connection)
        {
            command.Dispose();

            Console.WriteLine("Nouveau fournisseur:\nNuméro siret du fournisseur:");
            int siret = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Nom du fournisseur:");
            string nom = Console.ReadLine();

            Console.WriteLine("Adresse du fournisseur:");
            string adresse = Console.ReadLine();

            Console.WriteLine("Contact du fournisseur:");
            string contact = Console.ReadLine();

            Console.WriteLine("Libellé du fournisseur (1 très bon,2 bon, 3 moyen, 4 mauvais).:");
            int libele = Convert.ToInt32(Console.ReadLine());

            string insertTable = " INSERT INTO fournisseur  VALUES ('" + siret + "', '" + nom + "', '" + contact + "', '" + adresse + "', '" + libele + "');";
            command.CommandText = insertTable;
            try
            {
                command.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                Console.WriteLine(" ErreurConnexion : " + e.ToString());
                Console.ReadLine();
                return;
            }

            List<int> pieces = new List<int>();
            Console.WriteLine("Le fournisseur fournit-il des pièces:(true or false)");
            bool oui = Convert.ToBoolean(Console.ReadLine());
            while (oui == true)
            {
                Console.WriteLine("Quel n° de pièce fournit-il?");
                int piece = Convert.ToInt32(Console.ReadLine());
                pieces.Add(piece);
                Console.WriteLine("Fournit-il d'autres pièces: (true or false)");
                oui = Convert.ToBoolean(Console.ReadLine());
            }

            if (pieces != null)
            {
                foreach (int value in pieces)
                {
                    Console.WriteLine(value);
                    insertTable = " INSERT INTO fournit VALUES ('" + value + "', '" + siret + "'); ";
                    command.CommandText = insertTable;
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (MySqlException e)
                    {
                        Console.WriteLine(" ErreurConnexion : " + e.ToString());
                        Console.ReadLine();
                        return;
                    }
                }
            }

            Console.WriteLine("Le fournisseur " + nom + " a bien été ajouté");
        }

        static void Création_piece(MySqlCommand command, MySqlConnection connection)
        {
            int no_piece = 0;
            command.Dispose();
            MySqlDataReader reader;
            command.CommandText = "SELECT MAX(no_commande) FROM commande;";
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                no_piece = reader.GetInt32(0);
            }
            reader.Close();
            no_piece++;
            command.Dispose();
            command.Dispose();
            no_piece = 120;
            Console.WriteLine("Nouvelle pièce :\nQuel est le type de la pièce?");
            string descritpion = Console.ReadLine();

            Console.WriteLine("Quel est son numéro de catalogue?");
            string no_catalogue = Console.ReadLine();

            Console.WriteLine("Quel est son prix?");
            int prix = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Quel est son délai d'approvisionement?");
            int delai = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Quel est le numéro siret de son fournisseur?");
            long siret = Convert.ToInt64(Console.ReadLine());

            Console.WriteLine("Date d'introduction de la pièce");
            Console.WriteLine("Année");
            int annee = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Mois");
            int mois = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Jour");
            int jour = Convert.ToInt32(Console.ReadLine());

            DateTime date = new DateTime(annee, mois, jour);
            string Date10 = date.ToString("yyyy-MM-dd");

            Console.WriteLine("Date de discontinuation de la pièce");
            Console.WriteLine("Année");
            annee = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Mois");
            mois = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Jour");
            jour = Convert.ToInt32(Console.ReadLine());
            DateTime date2 = new DateTime(annee, mois, jour);
            string Date20 = date.ToString("yyyy-MM-dd");

            Console.WriteLine("En quelle quantté est disponible cette pièce?");
            int qt = Convert.ToInt32(Console.ReadLine());


            string insertTable = " INSERT INTO piece  VALUES ('" + no_piece + "', '" + descritpion + "', '" + no_catalogue + "', '" + prix + "', '" + Date10 + "', '" + Date20 + "', '" + delai + "', '" + qt + "');";
            command.CommandText = insertTable;
            try
            {
                command.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                Console.WriteLine(" ErreurConnexion : " + e.ToString());
                Console.ReadLine();
                return;
            }

            insertTable = " INSERT INTO fournit VALUES ('" + no_piece + "', '" + siret + "'); ";
            command.CommandText = insertTable;
            Console.WriteLine("La " + no_catalogue + " a bien été ajoutée");
        }
        static void Création_modèle(MySqlCommand command, MySqlConnection connection)
        {
            int no_piece = 0;
            command.Dispose();
            MySqlDataReader reader;
            command.CommandText = "SELECT MAX(no_commande) FROM commande;";
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                no_piece = reader.GetInt32(0);
            }
            reader.Close();
            no_piece++;
            command.Dispose();

            Console.WriteLine("Nouveau vélo :\nQuel est le nom du vélo?");
            string nom = Console.ReadLine();

            Console.WriteLine("Quelle est sa grandeur?");
            string taille = Console.ReadLine();

            Console.WriteLine("Quel est son type (ligne produit)?");
            int type = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Quel est son prix?");
            int prix = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Quel est son délai d'approvisionement?");
            int delai = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Quel est le numéro siret de son fournisseur?");
            long siret = Convert.ToInt64(Console.ReadLine());

            Console.WriteLine("Date d'introduction du modèle");
            Console.WriteLine("Année");
            int annee = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Mois");
            int mois = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Jour");
            int jour = Convert.ToInt32(Console.ReadLine());

            DateTime date = new DateTime(annee, mois, jour);
            string Date10 = date.ToString("yyyy-MM-dd");

            Console.WriteLine("Date de discontinuation du modèle");
            Console.WriteLine("Année");
            annee = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Mois");
            mois = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Jour");
            jour = Convert.ToInt32(Console.ReadLine());
            DateTime date2 = new DateTime(annee, mois, jour);
            string Date20 = date.ToString("yyyy-MM-dd");

            Console.WriteLine("En quelle quantité est disponible ce modèle?");
            int qt = Convert.ToInt32(Console.ReadLine());


            string insertTable = " INSERT INTO modele  VALUES ('" + no_piece + "', '" + nom + "', '" + taille + "', '" + prix + "', '" + Date10 + "', '" + Date20 + "', '" + delai + "', '" + type + "', '" + qt + "');";
            command.CommandText = insertTable;
            try
            {
                command.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                Console.WriteLine(" ErreurConnexion : " + e.ToString());
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Le" + nom + " a bien été ajouté");
        }


        static void Mode_Demo(MySqlCommand command, int choix, MySqlConnection maConnexion)
        {
            MySqlDataReader reader;

            switch (choix)
            {
                case 0:
                    break;
                case 1:
                    //1
                    Console.WriteLine("Nombre de clients:");
                    command.CommandText = "SELECT COUNT(*) FROM client;";
                    reader = command.ExecuteReader();
                    ReadSQL(reader);
                    
                    //2
                    Console.WriteLine("Noms des clients avec le cumul de toutes leurs commandes en euros:");
                    command.CommandText = "SELECT client.nom, SUM(COALESCE(quantite_piece, 0)*COALESCE(prix_piece, 0)+COALESCE(quantite_modele, 0)*COALESCE(prix_modele, 0)) AS total " +
                        "FROM commande c LEFT JOIN contient_modele mo ON c.no_commande=mo.no_commande LEFT JOIN contient_piece pi " +
                        "ON c.no_commande=pi.no_commande JOIN effectue e ON e.no_commande=c.no_commande JOIN client " +
                        "ON client.id_client=e.id_client LEFT JOIN piece p ON p.no_piece=pi.no_piece LEFT JOIN modele m " +
                        "ON m.no_modele=mo.no_modele GROUP BY e.id_client ORDER BY total DESC;";
                    reader = command.ExecuteReader();
                    ReadSQL(reader);
                    
                    //3
                    Console.WriteLine("Liste des produits ayant une quantité en stock <= 2:"); 
                    command.CommandText = "SELECT piece.no_catalogue, piece.description_piece, piece.stock_piece " +
                        "FROM piece WHERE (piece.stock_piece<=2) UNION SELECT modele.no_modele, modele.nom_modele,modele.stock_modele FROM modele " +
                        "WHERE (modele.stock_modele<=2);";
                    reader = command.ExecuteReader();
                    ReadSQL(reader);
                    

                    //4
                    Console.WriteLine("Nombre de pièces fournis par fournisseur:"); 
                    command.CommandText = "SELECT fournisseur.nom_fournisseur, COUNT(*) AS nombre_pieces_fournis " +
                        "FROM fournisseur INNER JOIN fournit ON fournisseur.siret=fournit.siret GROUP BY fournit.siret;";
                    reader = command.ExecuteReader();
                    ReadSQL(reader);
                    
                    //5
                    Console.WriteLine("Export en XML des pieces avec stock faible avec fournisseurs pour commande : ");
                    // XML :

                    // Création du document
                    XmlDocument docXml = new XmlDocument();

                    // Création de l'élément racine... qu'on ajoute au document
                    XmlElement racine = docXml.CreateElement("pieces");
                    docXml.AppendChild(racine);

                    // Création de l'en-tête XML (no <=> pas de DTD associée)
                    XmlDeclaration xmldecl = docXml.CreateXmlDeclaration("1.0", "UTF-8", "no");
                    docXml.InsertBefore(xmldecl, racine);

                    // Récupération des stocks faibles avec fournisseurs (pièces dont le stock est inférieur ou égal à 5)
                    // D'abord on récupère les pièces :
                    string requete = "SELECT piece.no_catalogue, piece.description_piece, piece.stock_piece FROM piece " +
                        "WHERE (piece.stock_piece<=5) GROUP BY piece.no_piece;";

                    MySqlCommand cmd = new MySqlCommand(requete, maConnexion);
                    reader = cmd.ExecuteReader();

                    // déclaration des tableaux :
                    int numRows = 0;

                    while (reader.Read())
                    {
                        numRows++;
                    }
                    reader.Close();

                    string[] pieces = new string[numRows];
                    string[] descriptions = new string[numRows];
                    string[] restants = new string[numRows];



                    cmd.CommandText = requete;
                    reader = cmd.ExecuteReader();

                    for (int i = 0; reader.Read(); i++)
                    {
                        pieces[i] = reader.GetValue(0).ToString();
                        descriptions[i] = reader.GetValue(1).ToString();
                        restants[i] = reader.GetValue(2).ToString();
                    }
                    reader.Close();

                    for (int i = 0; i < pieces.Length; i++)
                    {
                        // Ecriture des noeuds et des attributs concernant chaque pièce
                        XmlElement xml_piece = docXml.CreateElement("piece");
                        xml_piece.SetAttribute("code", pieces[i]);
                        XmlElement xml_description = docXml.CreateElement("description");
                        xml_description.InnerText = descriptions[i];
                        xml_piece.AppendChild(xml_description);
                        XmlElement xml_restants = docXml.CreateElement("en_stock");
                        xml_restants.InnerText = restants[i];
                        xml_piece.AppendChild(xml_restants);
                        XmlElement xml_fournisseurs = docXml.CreateElement("fournisseurs");

                        // Ensuite on récupère les fournisseurs qui vendent ces pièces (triés par délai de livraison)
                        cmd.CommandText = "SELECT fournisseur.siret, fournisseur.nom_fournisseur, piece.prix_piece, piece.delai_approvisionnement, fournisseur.libelle"
                            + " FROM fournisseur JOIN fournit ON fournit.siret = fournisseur.siret INNER "
                            + " JOIN piece ON piece.no_piece = fournit.no_piece "
                            + $"WHERE piece.no_catalogue = '{pieces[i]}' "
                            + "ORDER BY piece.prix_piece, piece.delai_approvisionnement,fournisseur.libelle;";
                        reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            // Ecriture des noeuds et des attributs concernant chaque fournisseurs
                            XmlElement xml_fournisseur = docXml.CreateElement("fournisseur");
                            xml_fournisseur.SetAttribute("siret", reader.GetString(0));
                            XmlElement xml_nom = docXml.CreateElement("nom");
                            xml_nom.InnerText = reader.GetString(1);
                            xml_fournisseur.AppendChild(xml_nom);
                            XmlElement xml_prix = docXml.CreateElement("prix");
                            xml_prix.InnerText = reader.GetString(2) + "€";
                            xml_fournisseur.AppendChild(xml_prix);

                            XmlElement xml_delai = docXml.CreateElement("delai");
                            xml_delai.SetAttribute("nb_Jours", reader.GetString(3));
                            xml_fournisseur.AppendChild(xml_delai);

                            XmlElement xml_libelle = docXml.CreateElement("libelle");
                            xml_libelle.InnerText = reader.GetString(4);
                            xml_fournisseur.AppendChild(xml_libelle);

                            xml_fournisseurs.AppendChild(xml_fournisseur);

                        }
                        reader.Close();
                        xml_piece.AppendChild(xml_fournisseurs);
                        racine.AppendChild(xml_piece);

                    }


                    // Sauvegarde du fichier et ouverture automatique du fichier 
                    docXml.Save("pieces_stock.xml");
                    Console.WriteLine("Veuillez trouver le fichier pieces_stock.xml dans le bin/debug du projet");
                    Console.ReadKey();


                    //6

                    Console.WriteLine("Export en JSON des clients dont le programme de fidélité arrive à expiration dans moins de 2 mois " +
                        "avec historique des abonnements afin de les relancer: ");
                    // JSON :


                    // Sélection des infos qui nous intéressent pour les clients dont la date d'expiration du programme fidélio est <= 2 mois
                    requete = "SELECT client_particulier.id_client, client.nom, client_particulier.prenom_particulier, programme.description_programme, " +
                        "adhere.Date_adhésion, date_add(adhere.Date_adhésion, INTERVAL programme.duree YEAR) AS expiration, programme.duree, " +
                        "programme.cout, CASE WHEN CURRENT_DATE BETWEEN adhere.date_adhésion AND DATE_ADD(adhere.date_adhésion, " +
                        "INTERVAL programme.duree year) THEN 'Abonné' WHEN adhere.date_adhésion IS NULL THEN 'Non abonné' ELSE 'Expiré' END AS statut " +
                        "FROM client_particulier JOIN client ON client_particulier.id_client = client.id_client LEFT JOIN adhere " +
                        "ON adhere.id_client = client.id_client JOIN programme ON adhere.no_programme = programme.no_programme " +
                        "WHERE DATE_ADD(CURRENT_DATE, INTERVAL 2 MONTH) > DATE_ADD(adhere.date_adhésion, INTERVAL programme.duree YEAR) " +
                        "GROUP BY client.id_client ORDER BY expiration DESC; ";


                    cmd = new MySqlCommand(requete, maConnexion);
                    reader = cmd.ExecuteReader();

                    // Création d'un writer et d'un json writer
                    StreamWriter writer = new StreamWriter("clients_a_relancer.json");
                    JsonTextWriter jw = new JsonTextWriter(writer);

                    jw.Formatting = Newtonsoft.Json.Formatting.Indented;

                    // Remplissage du json
                    jw.WriteStartObject();
                    jw.WritePropertyName("clients");
                    jw.WriteStartArray();
                    while (reader.Read())
                    {
                        jw.WriteStartObject();
                        jw.WritePropertyName("id");
                        jw.WriteValue(reader.GetUInt16(0));
                        jw.WritePropertyName("nom");
                        jw.WriteValue(reader.GetString(1));
                        jw.WritePropertyName("prenom");
                        jw.WriteValue(reader.GetString(2));
                        jw.WritePropertyName("programme");
                        jw.WriteStartArray();
                        jw.WriteStartObject();
                        jw.WritePropertyName("nom_Programme");
                        jw.WriteValue(reader.GetString(3));
                        jw.WritePropertyName("adhésion");
                        jw.WriteValue(reader.GetDateTime(4).Date);
                        jw.WritePropertyName("expiration");
                        jw.WriteValue(reader.GetDateTime(5).Date);
                        jw.WritePropertyName("duree");
                        jw.WriteValue(reader.GetUInt16(6));
                        jw.WritePropertyName("count");
                        jw.WriteValue(reader.GetUInt16(7));
                        jw.WritePropertyName("statut");
                        jw.WriteValue(reader.GetString(8));

                        jw.WriteEndObject();
                        jw.WriteEndArray();
                        jw.WriteEndObject();
                    }
                    jw.WriteEndArray();
                    jw.WriteEndObject();
                    reader.Close();

                    jw.Close();
                    writer.Close(); // fermeture

                    Console.WriteLine("Veuillez trouver le fichier clients_a_relancer.json dans le bin/debug du projet");
                    Console.ReadKey();
                    break;

            }

        }

        static void Requete_creation(MySqlCommand command, int choix) 
        {
            MySqlDataReader reader;
            switch (choix)
            {
                case 0:
                    break;
                case 1:
                    Console.WriteLine("Requête synchronisée:");
                    Console.WriteLine("Liste des dates d'introduction des modèles ayant un prix inférieur à la moyenne des modèles de la même ligne_produit : ");
                    command.CommandText = "SELECT M.date_introduction_modele FROM modele M WHERE M.prix_modele < (SELECT AVG(M1.prix_modele) " +
                        "FROM modele M1 WHERE M1.ligne_produit=M.ligne_produit) ;";
                    reader = command.ExecuteReader();
                    ReadSQL(reader);
                    break;
                case 2:
                    Console.WriteLine("Requête avec auto-jointure :");
                    Console.WriteLine("Liste des noms des fournisseurs qui ont le même libelle :");
                    command.CommandText = "SELECT F1.nom_fournisseur, F2.nom_fournisseur FROM Fournisseur F1, Fournisseur F2 WHERE F1.libelle=F2.libelle " +
                        "AND F1.nom_fournisseur < F2.nom_fournisseur;";
                    reader = command.ExecuteReader();
                    ReadSQL(reader);
                    break;
                case 3:
                    Console.WriteLine("Requête avec une union:");
                    command.CommandText = "SELECT * FROM client_particulier UNION SELECT * FROM client_entreprise;";
                    reader = command.ExecuteReader();
                    ReadSQL(reader);
                    break;
            }
        }


        static void Main(string[] args)
        {

            MySqlConnection maConnexion = null;
            try
            {
                string connexionString = "SERVER=localhost;PORT=3306;DATABASE=VeloMax;UID=root;PASSWORD=root;";

                maConnexion = new MySqlConnection(connexionString);
                maConnexion.Open();
            }
            catch (MySqlException e)
            {
                Console.WriteLine(" ErreurConnexion : " + e.ToString());
                return;
            }
            MySqlCommand command = maConnexion.CreateCommand();


            int numero = -1;

            while (numero != 0)
            {

                Console.WriteLine("Bienvenue sur le site de Vélo max. \nTapez 0 si vous voulez quitter ");
                Console.WriteLine("Tapez 1: Gestion des pièces de rechanges et Gestion des vélos");
                Console.WriteLine("Tapez 2: Gestion des clients particuliers et des clients Entreprise");
                Console.WriteLine("Tapez 3: Gestion des fournisseurs ");
                Console.WriteLine("Tapez 4: Gestion des commandes");
                Console.WriteLine("Tapez 5: Gestion des stocks");
                Console.WriteLine("Tapez 6: Statistiques du magasin");
                Console.WriteLine("Tapez 7 : Mode Démo");
                Console.WriteLine("Tapez 8 : Requêtes de création");

                numero = Convert.ToInt32(Console.ReadLine());
                int choix = 0;
                switch (numero)
                {
                    case 0:
                        break;
                    case 1:
                        Console.WriteLine("Que souhaitez-vous faire?\n0: Retour \n1: Créer une nouvelle pièce. \n2:Supprimer une pièce \n3: Modifier le prix d'une pièce \n4: Créer un nouveau vélo. \n5:Supprimer un vélo \n6: Modifier un vélo ");
                        choix = Convert.ToInt32(Console.ReadLine());
                        switch (choix)
                        {
                            case 0:
                                break;
                            case 1:
                                Création_piece(command, maConnexion);
                                break;
                            case 2:
                                Suppression_piece(command, maConnexion);
                                break;
                            case 3:
                                Maj_piece_prix(command, maConnexion);
                                break;
                            case 4:
                                Création_modèle(command, maConnexion);
                                break;
                            case 5:
                                Suppression_modele(command, maConnexion);
                                break;
                            case 6:
                                Maj_vélo_prix(command, maConnexion);
                                break;
                        }
                        break;
                    case 2:
                        Console.WriteLine("Votre client est?\n1: Un particulier. \n2: Une entreprise \n0:Retour");

                        choix = Convert.ToInt32(Console.ReadLine());
                        switch (choix)
                        {
                            case 1:
                                Console.WriteLine("Client particulier:\nQue souhaitez-vous faire?\n0: Retour\n1: Ajouter un nouveau client. \n2:Supprimer un client \n3: Modifier un client");
                                choix = Convert.ToInt32(Console.ReadLine());
                                switch (choix)
                                {
                                    case 0:
                                        break;
                                    case 1:
                                        Création_client_particulier(command, maConnexion);
                                        break;
                                    case 2:
                                        Suppression_client(command, maConnexion);
                                        break;
                                    case 3:
                                        Maj_client(command, maConnexion);
                                        break;
                                }

                                break;
                            case 2:
                                Console.WriteLine("Client entreprise:\nQue souhaitez-vous faire?\n0: Retour\n1: Ajouter un nouveau client. \n2:Supprimer un client \n3: Modifier un client");
                                choix = Convert.ToInt32(Console.ReadLine());
                                switch (choix)
                                {
                                    case 0:
                                        break;
                                    case 1:
                                        Création_client_entreprise(command, maConnexion);
                                        break;
                                    case 2:
                                        Suppression_client(command, maConnexion);
                                        break;
                                    case 3:
                                        Maj_client(command, maConnexion);
                                        break;
                                }
                                break;
                            case 0:
                                
                                break;
                        }
                        break;
                    case 3:
                        Console.WriteLine("Gestion des fournisseurs:\nQue souhaitez-vous faire?\n0: Retour\n1: Ajouter un nouveau fournisseur. \n2:Supprimer un fournisseur  \n3: Modifier un fournisseur");
                        choix = Convert.ToInt32(Console.ReadLine());
                        switch (choix)
                        {
                            case 0:
                                break;
                            case 1:
                                Création_fournisseur(command, maConnexion);
                                break;
                            case 2:
                                Suppression_fournisseur(command, maConnexion);
                                break;
                            case 3:
                                Maj_fournisseur(command, maConnexion);
                                break;
                        }
                        break;
                    case 4:
                        Console.WriteLine("Gestion de commande. \nQue souhaitez-vous faire?\n1: Créer une nouvelle commande. \n2:Supprimer une commande \n3: Modifier une commande");
                        choix = Convert.ToInt32(Console.ReadLine());
                        switch (choix)
                        {
                            case 1:
                                Création_commande(command, maConnexion);
                                break;
                            case 2:
                                Suppression_commande(command, maConnexion);
                                break;
                            case 3:
                                Maj_Commande(command, maConnexion);
                                break;
                        }
                        break;
                    case 5:
                        Console.WriteLine("Gestion du stock. \nQue souhaitez-vous connaitre?\n1:Stock des pièces. \n2:Stock des vélos \n0: Retour");
                        choix = Convert.ToInt32(Console.ReadLine());
                        switch (choix)
                        {
                            case 1:
                                Gestion_piece(command);
                                break;
                            case 2:
                                Gestion_modele(command);
                                break;
                            case 0:
                                break;
                        }
                        break;
                    case 6:
                        choix = 1;
                        while (choix != 0)
                        {
                            Console.WriteLine("Que souhaitez vous savoir?\n0: Retour \n1: Les quantités vendues \n2: La liste des membres pour chaque programme d'adhesion \n3:Date d’expiration des adhésions \n4: Le (ou les) meilleur client \n5: Analyse des commandes ");

                            choix = Convert.ToInt32(Console.ReadLine());
                            Rapport_stat(command, choix);
                        }
                        break;
                    case 7:
                        Console.WriteLine("Que souhaitez vous savoir?\n:0: Retour \n1: Nombre de clients \n2: Noms des clients avec le cumul de toutes ses commandes en euros \n3:Liste des produits ayant une quantité en stock <= 2 \n4: Nombres de pièces fournis par fournisseur. \n5: Export en XML / JSON d’une table ");

                        choix = Convert.ToInt32(Console.ReadLine());
                        while (choix != 0)
                        {

                            Mode_Demo(command, choix, maConnexion);
                        }

                        break;
                    case 8:
                        Console.WriteLine("Que souhaitez vous afficher?\n:0: Retour  \n1: Requête synchronisée \n2: Requête avec auto-jointure \n3: Requête avec une union");
                        choix = Convert.ToInt32(Console.ReadLine());
                        while (choix != 0)
                        {
                            Console.WriteLine("Que souhaitez vous afficher?\n:0: Retour  \n1: Requête synchronisée \n2: Requête avec auto-jointure \n3: Requête avec une union");

                            choix = Convert.ToInt32(Console.ReadLine());
                            Requete_creation(command, choix);
                        }

                        break;
                }
            }





        }
    }
}
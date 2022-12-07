using Newtonsoft.Json;
using System.Net.Mail;
using Reparation;
using System.Text.Json;

const string filename = "Repairs.txt";
string content;

if (!File.Exists(filename))
{
    content = "[]";
    File.WriteAllText(filename, content);
}
else
{
    content = File.ReadAllText(filename);
}

var listRepair = JsonConvert.DeserializeObject<List<TauxReparation>>(content);
Choice();

void Choice()
{
    Console.Write(@"=========== MENU REPARATION ===========

1 - Ajouter une réparation
2 - Voir les réparations
3 - Information
4 - Contact
5 - Imprimer
6 - Quitter

Quel est votre choix ? ");

    string result = Console.ReadLine().ToLower();

    switch(result)
    {
        case "1":
        case "ajouter":
            AddRepair();
            break;
        case "2":
        case "voir":
            SeeRepairChoice();
            break;
        case "3":
        case "info":
        case "information":
            Information();
            break;
        case "4":
        case "contact":
            //SendMail("smtp.gmail.com");
            break;
        case "5":
        case "imprimer":
            //PrintRate();
            break;
        case "6":
        case "quitter":
            Exit();
            break;
        default:
            Console.Clear();
            Choice();
            break;
    }
}

void AddRepair()
{
    string card = AskTheUser("\nNom du produit: ", vString: CheckValidationName).ToUpper();
    string repairer = AskTheUser("Trigramme du réparateur: ", vString: CheckValidationTrigram).ToUpper();
    string strDate = AskTheUser("Date réparation (\"jj/mm/aaaa\"): ", vString: CheckValidationDate);
    if (string.IsNullOrWhiteSpace(strDate))
        strDate = DateTime.Now.ToString("dd/MM/yyyy");
    var date = new DateTime();
    string description = AskTheUser("Description: ");
    int okCard = int.Parse(AskTheUser("Nombre de carte ok: ", vString: CheckValidationNumber));
    int failCard = int.Parse(AskTheUser("Nombre de carte en erreur: ", vString: CheckValidationNumber));
    
    try 
    {
        date = DateTime.Parse(strDate);
    }
    catch
    {
        strDate = DateTime.Now.ToString("dd/MM/yyyy");
        date = DateTime.Parse(strDate);
    }

    if (string.IsNullOrEmpty(repairer))
        repairer = "AAA";
    if (string.IsNullOrEmpty(description))
        listRepair.Add(new TauxReparation(repairer, card, okCard, failCard, date));
    else
        listRepair.Add(new TauxReparation(repairer, card, okCard, failCard, date, description));

    string json = JsonConvert.SerializeObject(listRepair);

    File.WriteAllText(filename, json);
    Console.Beep();
    Console.Clear();

    Choice();
}

void SeeRepairChoice()
{
    Console.Clear();

    Console.Write(@"===== TEMPORALITE DES REPARATIONS =====

1 - Hebdomadaire
2 - Mensuel
3 - Annuel
4 - Toute les réparations
5 - Menu principal

Quel est votre choix ? ");

    string result = Console.ReadLine().ToLower();

    switch (result)
    {
        case "1":
        case "hebdomadaire":
            SeeRepair("1");
            break;
        case "2":
        case "mensuel":
            SeeRepair("2");
            break;
        case "3":
        case "annuel":
            SeeRepair("3");
            break;
        case "4":
        case "tout":
            SeeRepair("4");
            break;
        case "5":
        case "Menu principal":
            Console.Clear();
            Choice();
            break;
        default:
            Console.Clear();
            SeeRepairChoice();
            break;
    }
}

void SeeRepair(string choice)
{
    Console.Clear();

    if (listRepair != null)
    {
       DateTime today = DateTime.Now;
        var newRepair = new List<TauxReparation>();
        switch(choice)
        {
            case "1":
                newRepair = listRepair.Where(x => x.DateTime >= today.AddDays(-7)).ToList();
                break;
            case "2":
                newRepair = listRepair.Where(x => x.DateTime >= today.AddMonths(-1)).ToList();
                break;
            case "3":
                newRepair = listRepair.Where(x => x.DateTime >= today.AddMonths(-12)).ToList();
                break;
            case "4":
                newRepair = listRepair;
                break;
            default:
                Console.WriteLine("ERROR: LE PROGRAMME N'EMPRUNTE PAS UN DES 4 CHEMINS POSSIBLE (...TEMPORALITE DES REPARATIONS...)\n");
                break;
        }

              var sortedListRepair = from repair in newRepair
                                       orderby (repair.Ok + repair.Failure)
                                       orderby repair.Card
                                       orderby repair.DateTime
                                       group repair by repair.Operator into newGroup
                                       orderby newGroup.Key
                                       select newGroup;

        Console.WriteLine("========= LISTE DES REPARATION =========\n");

        foreach (var repair in sortedListRepair)
        {
            var avgOk = repair.Select(x => x.Ok).Average();
            var avgFail = repair.Select(x => x.Failure).Average();
            var sumOK = repair.Select(x => x.Ok).Sum();
            var sumFail = repair.Select(x => x.Failure).Sum();

            ColorString($"  OPERATEUR: {repair.Key}\n", ConsoleColor.Yellow);

            foreach (var item in repair)
            {
                ColorString($"Carte: {item.Card}", ConsoleColor.DarkYellow);
                if (item.Description != null)
                    ColorString($"Description: {item.Description}", ConsoleColor.DarkGray);
                GetTwicePercent(item.Ok, item.Failure);
                ColorString($"Date: {item.DateTime.ToString("dd/MM/yyyy")}", ConsoleColor.Cyan);
                Console.WriteLine();
            }
            ColorString($"Qté OK {repair.Key}: {sumOK}", ConsoleColor.Yellow);
            ColorString($"SCORE {repair.Key}: {avgOk * 100 / (avgOk + avgFail):0.00}%\n", ConsoleColor.Yellow);

            Console.WriteLine(new String('=', 40));
            Console.WriteLine();
        }
    }
    else
        Console.WriteLine("...La liste est vide...\n");

    ColorString("...Presser une touche pour continuer...");
    Console.ReadLine();
    Console.Clear();
    Choice();
}

void Information()
{
    Console.Clear();
    Console.WriteLine("============= INFORMATION =============\n\n" +
        "   Dans le \"MENU REPARATION\" (menu principal)\n" +
        "Pour la selection, vous pouvez utiliser:\n" +
        "- Soit des chiffres (1, 2, 3, 4)\n" +
        "- Soit des mots clefs (ajouter, voir, info ou information, contact, quitter).\n" +
        "Les mots ne sont d'ailleurs pas sensible à la casse (minuscule ou majuscule).\n\n" +
        "   Dans \"Ajouter une réparation\",\nsi vous ne rentrez pas la date de réparation, ou si elle est n'est pas valide,\n" +
        "elle sera automatiquement ajuster a la date actuelle.\n\n" +
        "   Dans \"Voir les réparations\", les cartes sont triées: \n" +
        "- Par \"Opérateur\"\n" +
        "- Puis par \"Date\"\n" +
        "- Puis par \"Carte\"\n" +
        "- Puis par \"Quantité totale\".\n");
    Console.WriteLine(new String('=', 39));

    Console.WriteLine("\n...Presser une touche pour continuer...");
    Console.ReadLine();
    Console.Clear();
    Choice();
}

void SendMail(string server)
{
    string from = AskTheUser("Entrez votre adresse: ");
    string to = "olivierbalmet@gmail.com";
    string subject = AskTheUser("Entrez le sujet du message: ");
    string body = AskTheUser("Ecriver ci dessous votre message: \n");

    var message = new MailMessage(from, to, subject, body);
    message.Subject = subject;
    message.Body = body;
    message.IsBodyHtml = false;
    SmtpClient client = new SmtpClient(server);
    client.UseDefaultCredentials = true;

    try
    {
        client.Send(message);
        Console.Beep();
    }
    catch(Exception ex)
    {
        Console.WriteLine(ex);
    }

    Choice();
}

void PrintRate()
{
    // Add Function
}

void Exit()
{
    return;
}

static string AskTheUser(string message, ValidationString? vString = null)
{
    Console.Write($"{message} ");
    string answer = Console.ReadLine();

    if (vString != null)
    {
        string error = vString(answer);
        if (error != null)
        {
            Console.WriteLine($"Erreur: {error}");
            return AskTheUser(message, vString);
        }
    }
    return answer;
}

static string? CheckValidationTrigram(string str)
{
    if (string.IsNullOrWhiteSpace(str))
        return null;

    if (str.Length != 3 || str.Any(char.IsDigit))
        return "Le trigramme doit être composé de 3 lettres: la 1ère lettre du prénom et les 2 premières lettre du nom)";
    
    return null;
}

static string? CheckValidationName(string str)
{
    if(string.IsNullOrWhiteSpace(str))
        return "Le nom ne peut être vide";
    
    if(str.All(char.IsDigit))
        return "Le nom ne doit pas contenir que des chiffres";
    
    return null;
}

static string? CheckValidationNumber(string str)
{
    if(string.IsNullOrWhiteSpace(str))
        return "Le nombre ne peut pas être vide";
    
    if(!str.All(char.IsDigit))
        return "Le nombre ne peux contenir que des chiffres";
    
    return null;
}

static string? CheckValidationDate(string str)
{
    if (string.IsNullOrWhiteSpace(str))
        return null;
    if (str.Length != 10)
        return "Le format doit être ainsi: \"jj/mm/aaaa\"";

    return null;
}

static void ColorString(string message, ConsoleColor color = ConsoleColor.Gray)
{
    Console.ForegroundColor = color;
    Console.WriteLine(message);
    Console.ResetColor();
}

static void GetTwicePercent(float ok, float bd)
{
    Console.WriteLine($"Total: {ok + bd} unitée(s)");
    Console.Write("Ok: ");
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write($"{ok} unitée(s) ");
    Console.ResetColor();
    Console.Write("(soit ");
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write($"{ ok * 100 / (ok + bd):0.##}%");
    Console.ResetColor();
    Console.WriteLine(")");    
    Console.Write("Erreur: ");
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write($"{bd} (unitée(s)");
    Console.ResetColor();
    Console.Write(" (soit ");
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write($"{bd * 100 / (ok + bd):0.##}%");
    Console.ResetColor();
    Console.WriteLine(")");
    
}

enum Reparator
{
    ELE,
    MLA,
    OBA
}

enum Card
{
    AFFICHEUR,
    ALIMENTATION,
    BOUTTON_D_APPEL,
    TELERELEVE,
    CCD,
    CLAVIER,
    COB,
    DAACO,
    DFU,
    DOG,
    GSM,
    IRB,
    IRPV3,
    MEDAILLION,
    MIA,
    MIA_TOP,
    PASSERELLE,
    RRA,
    SIRENE,
    TELECOMMANDE,
}

public delegate string ValidationString(string str);
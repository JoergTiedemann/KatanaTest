using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

/* 
 * Klassensammlung in der die Kommunikationsklassen für den Datentransfer zum Auftragsserver festgelegt wird
 */

namespace DModul
{
#pragma warning disable 169
    public class GlobConst
    {   
        public const int EinPlaBeg = 1;
        public const int EinPlaEnd = 10;
        public const int FunktAnz = 17;
        public const int NFnktAnz = 7;
        public const int SaegeAnz = 3;
    }

    public struct PFunkt
    {
        public int Flag;				// Funktion ist aktiv
        public int DFlag;				// Funktion ist aktiv aber kann nichts machen (Zielplatz belegt, usw.)
        public int WillFlag;			// Funktion möchte aktiv sein
        public int Umsetz;				// Funktion setzt um (um an ein Teil ranzukommen)
        public int WarteFlag;			// siehe Doku auf X:\E_KONST\TEXTE\INTERNA\SCHULUNG\Tlf\WarteFlag_TLF.pdf
        public int HubtischWaitState;  // Warte-Status für Hubtisch
        public int Freigelegt;			// Diese Funktion hat auf diesen Platz ein Teil freigelegt (der Ausstapelplatz ist nicht bereit)
        public int WarteVorFrg;		// Vorabfreigabe von Holzma für Auslageraufträge
        public int FFStarten;			// für die Datenausgabe der Pufferstation hinter den Ausstapelplätzen
        public int WarteRestFlag;		// Auslagerauftrag für Restteil vorhanden, Anzahl für die Restteile aber noch nicht da.
        public int WaitState;			// Warte-Status
        public void LoadFromArray(byte[] array,int index)
        {
            Flag = BitConverter.ToInt32(array, index+0);
            DFlag = BitConverter.ToInt32(array, index + sizeof(int));
            WillFlag = BitConverter.ToInt32(array, index + sizeof(int) * 2);
            Umsetz = BitConverter.ToInt32(array, index + sizeof(int) * 3);
            WarteFlag = BitConverter.ToInt32(array, index + sizeof(int) * 4);
            HubtischWaitState = BitConverter.ToInt32(array, index + sizeof(int) * 5);
            Freigelegt = BitConverter.ToInt32(array, index + sizeof(int) * 6);
            WarteVorFrg = BitConverter.ToInt32(array, index + sizeof(int) * 7);
            WarteRestFlag = BitConverter.ToInt32(array, index + sizeof(int) * 8);
            WaitState = BitConverter.ToInt32(array, index + sizeof(int) * 9);
        }

    };

    public class sFlagTab
    {
		PFunkt[]	PFTab;						// Lagerfunktionen, die Teile bewegen (Einlagern, Auslager, usw.)
		PFunkt[]	NFTab;						// Funktionen, um Optionen zu aktivieren (z.B. Sägenkommunikation starten)
		int[]		PufferStatus;				// Für Puffer hinter den Ausstapelplätzen
		int		OFunktion;								// Die aktuelle Funktionen des Fahrprozesses
		int		Funktion;								// Die Funktion vom Auftragprozess
		int		Rueckmeld;								// Status über Stand des Auftrags (vorm Ansaugen, nach dem Ansaugen, usw.)
        string  AktTeilId;    							// Das aktuelle Teil, was bewegt wird (Identnummer)
		int		AktTeilVon;								// Quellplatz (Übergabewert ans Fahrprogramm)
		int		AktTeilZu;								// Zielplatz (Übergabewert ans Fahrprogramm)
		int		Fahrstate;								// Fahrstatus des Fahrprogramms (aufnehmen, wartet auf aufgenommen, usw.)
		int		IstFahrVon,IstFahrZu;					// Quell und Zielplatz (Rückmeldung)
		int		AktFKTraverse;							// Festkante der Traverse, wo das Teil hängt
		int		LeerPlatzAnzahl;						// Verbleibende Anzahl bei Platzleeren-Funktion
		int		LeerPlatzAnzahlMax;						// Ursprüngliche Anzahl bei Platzleeren-Funktion
		int		UmstapelAnzahl;							// Anzahl Umstapelungen bis gesuchtes Teil oben liegt
		int		UmstapelAnzahlMax;						// Ursprüngliche Anzahl Umstapelungen bis gesuchtes Teil oben liegt
		int		AktUrLaenge;							// Die Länge des Teils, welches bewegt wird
		int		AktUrBreite;							// Die Breite des Teils, welches bewegt wird
		long	X1Ist;									// Istwerte von X-Achse 1
		long	YIst;									// Istwerte von Y-Achse
		long	X2Ist;									// Istwerte von X-Achse 2
		long	ZIst;									// Istwerte von Z-Achse
		bool	TraverseBelegt;							// Traverse ist belegt
		long	ZSoll;									// Sollwert für den Hub
		long	DrehPos;								// Istwert für den Drehantrieb
		bool	ShowFahrstate;							// Merker, ob der Fahrstatus im Lager an ist
		int[]	StrBetrieb;					// Merker für die Sägen (Farbvisualisierung)
		bool	Automatik;								// SPS ist im Automatikbetrieb
		bool	FahrAbbruch;							// Abbruch läuft am Fahrprogramm
		bool	Kranleeren;								// Kranlleren laeuft
		long	IstGewicht;								// Istwerte vom Gewicht
        bool[]	SPSIO;
		bool[]	EinAbraeumen; // fuer Einlagerplaetze

        public sFlagTab()
        {
	    	PFTab = new PFunkt[GlobConst.FunktAnz+1];
    		NFTab = new PFunkt[GlobConst.NFnktAnz+1];
		    EinAbraeumen = new bool[(GlobConst.EinPlaEnd-GlobConst.EinPlaBeg)+2];
		    PufferStatus = new int[GlobConst.SaegeAnz+1];
		    StrBetrieb = new int[GlobConst.SaegeAnz+1];
            SPSIO = new bool[1024];
        }
        private int LoadNextInt(byte[] array, ref int aktIndex)
        {
            int i = BitConverter.ToInt32(array, aktIndex);
            aktIndex = aktIndex + sizeof(int);
            return i;
        }
        private bool LoadNextbool(byte[] array, ref int aktIndex)
        {
            bool i = BitConverter.ToBoolean(array, aktIndex);
            aktIndex = aktIndex + sizeof(int);// da in Cpp der BOOL gemeint ist und der 4 byte ist
            return i;
        }

        public void LoadFromArray(byte[] array)
        {
            int aktIndex = 0;
            PFunkt pf = new PFunkt();
            for (int i = 0; i <= GlobConst.FunktAnz; i++)
            {
                PFTab[i].LoadFromArray(array, aktIndex);
                aktIndex = aktIndex + Marshal.SizeOf(pf);
            }
            for (int i = 0; i <= GlobConst.NFnktAnz; i++)
            {
                NFTab[i].LoadFromArray(array, aktIndex);
                aktIndex = aktIndex + Marshal.SizeOf(pf);
            }
            for (int i = 0; i <= GlobConst.SaegeAnz; i++)
            {
                PufferStatus[i] = LoadNextInt(array, ref  aktIndex);
            }
            OFunktion = LoadNextInt(array, ref  aktIndex);
            Funktion = LoadNextInt(array, ref aktIndex);
            Rueckmeld = LoadNextInt(array, ref aktIndex);
            AktTeilId = Encoding.Unicode.GetString(array, aktIndex, 512);
            aktIndex = aktIndex + 512;
            AktTeilVon = LoadNextInt(array, ref aktIndex);
            AktTeilZu = LoadNextInt(array, ref aktIndex);
            Fahrstate = LoadNextInt(array, ref aktIndex);
            IstFahrVon = LoadNextInt(array, ref  aktIndex);
            IstFahrZu = LoadNextInt(array, ref  aktIndex);
            AktFKTraverse = LoadNextInt(array, ref  aktIndex);
            LeerPlatzAnzahl = LoadNextInt(array, ref  aktIndex);
            LeerPlatzAnzahlMax = LoadNextInt(array, ref  aktIndex);
            UmstapelAnzahl = LoadNextInt(array, ref  aktIndex);
            UmstapelAnzahlMax = LoadNextInt(array, ref  aktIndex);
            AktUrLaenge = LoadNextInt(array, ref  aktIndex);
            AktUrBreite = LoadNextInt(array, ref  aktIndex);
            X1Ist = LoadNextInt(array, ref  aktIndex);		
            YIst = LoadNextInt(array, ref  aktIndex);
            X2Ist = LoadNextInt(array, ref  aktIndex);
            ZIst = LoadNextInt(array, ref  aktIndex);
            TraverseBelegt = LoadNextbool(array, ref  aktIndex);
            ZSoll = LoadNextInt(array, ref  aktIndex);		
            DrehPos = LoadNextInt(array, ref  aktIndex);
            ShowFahrstate = LoadNextbool(array, ref  aktIndex);
            for (int i = 0; i <= GlobConst.SaegeAnz; i++)
            {
                StrBetrieb[i] = LoadNextInt(array, ref  aktIndex);
            }
            Automatik = LoadNextbool(array, ref  aktIndex);
            FahrAbbruch = LoadNextbool(array, ref  aktIndex);
            Kranleeren = LoadNextbool(array, ref  aktIndex);;	
            IstGewicht = LoadNextInt(array, ref  aktIndex);
            for (int i = 0; i < 1024; i++)
            {
                SPSIO[i] = LoadNextbool(array, ref  aktIndex);
            }
            for (int i = 0; i <= (GlobConst.EinPlaEnd - GlobConst.EinPlaBeg); i++)
            {
                EinAbraeumen[i] = LoadNextbool(array, ref  aktIndex);
            }
        }
    }
#pragma warning restore 169
}
                                                                     
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSequence : MonoBehaviour {
	public static readonly string Sequence = "RUHUHCCUUSRHSXEEJSJVVUHSVRVHCRCJVXJHJEEVESCCRJSJCESERJCXRHCXXURXEEHURHRSJEXJVRHUUJUUERESHXSUEHSJRUUSCXHUVJCVUEEHJXSVHXJHEJRSEUHXRJUCRHHXXHRJXHVXRRJCHXRXAJUCVSUJCVXVJYOOAUOECECMASVMETTUAMAEYYMEOEUTCOASYMMMOOUAYSYOUCOEMEEYTATACMCTTMTGEAOSYGAGTSOMUYEUSTEAGYETUTGEEMMMTAMSOAESUYAGSGYMASSSGAGMOMEGMAATBAEYMBTAAOUYOAYYETAMOWOBMMETETYTAMYAYATOTTAZMZBYTYTTZBOAOMZWTMOZWMGZOWTGMWMOOWAOAYTGMYWWYYGOOWOAMTWAWMWYGYWMAWBAWBOMMOTBWBOTBTAWYOZBTZABBBMAWMGWGTTTGTWWDOWMTGOTZQOGQWBODYIZWIWQDQQGWWDGOYYYQDOWFYZDGBZDYWBFZDWDQIZIFGFIBIBYYDGZBIFBYYWDGDIIBBGDZWBDDWZQIBDLIWIDWQWDLZGFGDFIZBBWZWIZLZZWFZGKZKZLKLQQZDKFKLQGKDKZDGDGZLLIIQKFBGGZLLGGBZIBBKIFBZBZKFLIDKKKDBDGFFILBQFKLLZQIKQFBBZZLLDFIGIQILBFDPFKKIILFNFDDLNLIDDZNNPZLPKKQNPZPLPKNIDKLVFVQNQQPPNVNFNKINILPPQDQKFPPQQINKVKFNINFKDPPVLKPLINLLVDVILPQVKDFDFLXILNLFQPFFLIXPPQNLNVIQPXFKQVLILFLNKKNKKNFPKLXKLKKIJRQKRVPQKQXNFQXJPPCQJXFCCQNXNJQRNJQQXXPHFNHRRCXRRNPHRHHJFVPNRCNVXNNCVJPJCHNCNNSSCPCJJNPPHNHJXSSRRVRHSVNNPXRNSRVCPSXRUXPSCSJHJXJPRXRVSVCUVHHHUUHPVHCSURCPVJUVXPUXHHUCCJ";

	// todo: tHEse could probably all be automatically populated. It would make making changes to this easier. Probably not gonna bother until such a change is actually needed.

	public static readonly Dictionary<char, int> Firsts = new Dictionary<char, int>() {
		{'Y',165},
		{'B',296},
		{'C',852},
		{'D',448},
		{'E',014},
		{'F',488},
		{'O',166},
		{'H',873},
		{'I',466},
		{'J',834},
		{'K',579},
		{'L',547},
		{'M',175},
		{'N',685},
		{'G',231},
		{'P',677},
		{'Q',457},
		{'R',835},
		{'S',912},
		{'T',181},
		{'U',950},
		{'V',717},
		{'W',317},
		{'X',784},
		{'A',152},
		{'Z',339},
	};

	public static readonly Dictionary<char, int> Lasts = new Dictionary<char, int>() {
		{'Y',525},
		{'B',674},
		{'C',226},
		{'D',781},
		{'E',324},
		{'F',890},
		{'O',486},
		{'H',148},
		{'I',833},
		{'J',164},
		{'K',842},
		{'L',830},
		{'M',451},
		{'N',941},
		{'G',669},
		{'P',991},
		{'Q',869},
		{'R',150},
		{'S',283},
		{'T',455},
		{'U',306},
		{'V',178},
		{'W',575},
		{'X',162},
		{'A',435},
		{'Z',707},
	};

	public static readonly Dictionary<char, char> Next = new Dictionary<char, char>() {
		{'Y','L'},
		{'B','N'},
		{'C','G'},
		{'D','X'},
		{'E','Z'},
		{'F','S'},
		{'O','F'},
		{'H','A'},
		{'I','R'},
		{'J','M'},
		{'K','C'},
		{'L','J'},
		{'M','Q'},
		{'N','U'},
		{'G','P'},
		{'P','E'},
		{'Q','H'},
		{'R','Y'},
		{'S','B'},
		{'T','I'},
		{'U','W'},
		{'V','T'},
		{'W','K'},
		{'X','O'},
		{'A','D'},
		{'Z','V'},
	};

	public static readonly Dictionary<char, char> Previous = new Dictionary<char, char>() {
		{'Y','R'},
		{'B','S'},
		{'C','K'},
		{'D','A'},
		{'E','P'},
		{'F','O'},
		{'O','X'},
		{'H','Q'},
		{'I','T'},
		{'J','L'},
		{'K','W'},
		{'L','Y'},
		{'M','J'},
		{'N','B'},
		{'G','C'},
		{'P','G'},
		{'Q','M'},
		{'R','I'},
		{'S','F'},
		{'T','V'},
		{'U','N'},
		{'V','Z'},
		{'W','U'},
		{'X','D'},
		{'A','H'},
		{'Z','E'},
	};
}

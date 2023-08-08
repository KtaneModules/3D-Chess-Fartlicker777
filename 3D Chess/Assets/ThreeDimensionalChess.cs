using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using Math = ExMath;

public class ThreeDimensionalChess : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;

   public KMSelectable[] CoordinateButtons;
   public KMSelectable[] SubmitButtons;

   public TextMesh[] SubmitDisplays;
   public TextMesh CoordinateDisplay;

   string[][] CoordinateNotation = new string[][] {
      new string[] { "I", "II", "III", "IV", "V"},
      new string[] { "A", "B", "C", "D", "E"},
      new string[] { "1", "2", "3", "4", "5"},
   };
   string[] PieceTypes = { "King", "Knight", "Bishop", "Rook", "Queen"};
   string[] PieceTypesAbbr = { "K", "N", "B", "R", "Q" };
   int[] SubmitCoordinateIndices = { 0, 0, 0, 0};
   int LastPressed = -1;

   int[][][] Board = new int[][][] {
      new int[][] {
         new int[] { 0, 0, 0, 0, 0},
         new int[] { 0, 0, 0, 0, 0},
         new int[] { 0, 0, 0, 0, 0},
         new int[] { 0, 0, 0, 0, 0},
         new int[] { 0, 0, 0, 0, 0},
      },

      new int[][] {
         new int[] { 0, 0, 0, 0, 0},
         new int[] { 0, 0, 0, 0, 0},
         new int[] { 0, 0, 0, 0, 0},
         new int[] { 0, 0, 0, 0, 0},
         new int[] { 0, 0, 0, 0, 0},
      },

      new int[][] {
         new int[] { 0, 0, 0, 0, 0},
         new int[] { 0, 0, 0, 0, 0},
         new int[] { 0, 0, 0, 0, 0},
         new int[] { 0, 0, 0, 0, 0},
         new int[] { 0, 0, 0, 0, 0},
      },

      new int[][] {
         new int[] { 0, 0, 0, 0, 0},
         new int[] { 0, 0, 0, 0, 0},
         new int[] { 0, 0, 0, 0, 0},
         new int[] { 0, 0, 0, 0, 0},
         new int[] { 0, 0, 0, 0, 0},
      },

      new int[][] {
         new int[] { 0, 0, 0, 0, 0},
         new int[] { 0, 0, 0, 0, 0},
         new int[] { 0, 0, 0, 0, 0},
         new int[] { 0, 0, 0, 0, 0},
         new int[] { 0, 0, 0, 0, 0},
      }
   };

   int[][] Positions = new int[][] {
      new int[] { 0, 0, 0},
      new int[] { 0, 0, 0},
      new int[] { 0, 0, 0},
      new int[] { 0, 0, 0},
      new int[] { 0, 0, 0},
      new int[] { 0, 0, 0},
      new int[] { 0, 0, 0},
   };

   Piece[] Pieces = {
      new Piece(), new Piece(), new Piece(), new Piece(), new Piece(), new Piece(), new Piece()
   };

   string FinalPieceType = "";

   Piece Eighth = new Piece();
   public PieceBehavior PB;

   int Offset = 0;

   int ULTRATIMWI = 0;

   static int ModuleIdCounter = 1;
   int ModuleId;
   private bool ModuleSolved;

   void Awake () {
      ModuleId = ModuleIdCounter++;
      GetComponent<KMBombModule>().OnActivate += Activate;
      
      foreach (KMSelectable Button in CoordinateButtons) {
          Button.OnInteract += delegate () { CButPress(Button); return false; };
      }

      foreach (KMSelectable Button in SubmitButtons) {
         Button.OnInteract += delegate () { SButPress(Button); return false; };
      }

   }

   void CButPress (KMSelectable C) {
      if (ModuleSolved) {
         return;
      }
      for (int i = 0; i < 7; i++) {
         if (C == CoordinateButtons[i]) {
            StartCoroutine(KeyPress(i));
            if (LastPressed != -1) {
               StartCoroutine(KeyDepress(LastPressed));
            }
            LastPressed = i;
            int temp = i - Offset;
            if (temp < 0) {
               temp += 7;
            }
            UpdateCoordinateDisplay(temp);
         }
      }
   }

   void SButPress (KMSelectable S) {
      Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
      if (ModuleSolved) {
         return;
      }
      for (int i = 0; i < 5; i++) {
         if (S == SubmitButtons[i]) {
            if (i == 4) {
               Piece Test = new Piece(SubmitCoordinateIndices[1], SubmitCoordinateIndices[2], SubmitCoordinateIndices[3], PieceTypes[SubmitCoordinateIndices[0]]);
               if (PB.CheckPiece(Pieces[6].T, Pieces[6].L, Pieces[6].R, Pieces[6].C, Test.L, Test.R, Test.C) && PB.CheckPiece(Test.T, Test.L, Test.R, Test.C, Pieces[0].L, Pieces[0].R, Pieces[0].C) && !CheckDuplicateCoordinatesForAll(Test)) {
                  Solve();
                  if (LastPressed != -1) {
                     StartCoroutine(KeyDepress(LastPressed));
                     CoordinateDisplay.text = "!";
                  }
                  else {
                     CoordinateDisplay.text = "sure";
                  }
               }
               else {
                  Strike();
                  Debug.LogFormat("[3D Chess #{0}] Submitting a {1} at {2}{3}{4} is incorrect.", ModuleId, Test.T, CoordinateNotation[0][Test.L], CoordinateNotation[1][Test.C], CoordinateNotation[2][Test.R]);
                  if (PB.CheckPiece(Pieces[6].T, Pieces[6].L, Pieces[6].R, Pieces[6].C, Test.L, Test.R, Test.C)) {
                     Debug.LogFormat("[3D Chess #{0}] The seventh piece does not attack the eighth.", ModuleId);
                  }
                  if (PB.CheckPiece(Test.T, Test.L, Test.R, Test.C, Pieces[0].L, Pieces[0].R, Pieces[0].C)) {
                     Debug.LogFormat("[3D Chess #{0}] The eighth piece does not attack the first.", ModuleId);
                  }
               }
            }
            else {
               SubmitCoordinateIndices[i] = (SubmitCoordinateIndices[i] + 1) % 5;
               UpdateSubmitDisplay();
            }
         }
      }
   }

   void UpdateCoordinateDisplay (int i) {
      CoordinateDisplay.text = PieceTypesAbbr[Array.IndexOf(PieceTypes, Pieces[i].T)] + CoordinateNotation[0][Pieces[i].L] + CoordinateNotation[1][Pieces[i].C] + CoordinateNotation[2][Pieces[i].R];
   }

   void UpdateSubmitDisplay () {
      SubmitDisplays[0].text = PieceTypesAbbr[SubmitCoordinateIndices[0]];
      for (int i = 0; i < 3; i++) {
         SubmitDisplays[i + 1].text = CoordinateNotation[i][SubmitCoordinateIndices[i + 1]];
      }
   }

   void OnDestroy () { //Shit you need to do when the bomb ends
      
   }

   void Activate () { //Shit that should happen when the bomb arrives (factory)/Lights turn on

   }

   void Start () { //Shit
      Offset = Rnd.Range(0, 7);

      SubmitDisplays[0].text = "K";
      for (int i = 0; i < 3; i++) {
         SubmitDisplays[i + 1].text = CoordinateNotation[i][0];
      }
      //This resets all the submit displays to their default state.

      GeneratePuzzle();
      CoordinateDisplay.text = "";
   }

   void LogPieces () {
      //Debug.Log(Offset);
      for (int i = 0; i < 7; i++) {
         Debug.LogFormat("[3D Chess #{0}] Piece {1} is a {2} at {3}{4}{5}. This is displayed on button {6}.", ModuleId, i + 1, Pieces[i].T, CoordinateNotation[0][Pieces[i].L], CoordinateNotation[1][Pieces[i].C], CoordinateNotation[2][Pieces[i].R], (Offset + i) % 7 + 1);
      }

      Debug.LogFormat("[3D Chess #{0}] The final piece is a {1} at {2}{3}{4}.", ModuleId, Eighth.T, CoordinateNotation[0][Eighth.L], CoordinateNotation[1][Eighth.C], CoordinateNotation[2][Eighth.R]);
   }

   void GeneratePuzzle () {
      int Timwi = 0;
      ULTRATIMWI++;
      if (ULTRATIMWI > 10) {
         CoordinateDisplay.text = "ERROR";
         Solve();
      }

      Pieces[0].ChangePiece(Rnd.Range(0, 5), Rnd.Range(0, 5), Rnd.Range(0, 5), PieceTypes[Rnd.Range(0, 5)]);   //Gets our starting piece, can be anything we want

      for (int i = 1; i < 7; i++) {
         RestartPiece:
         Timwi++;
         if (Timwi == 100000) {  //If it is impossible to place the next piece, we just restart the puzzle.
            GeneratePuzzle();
            return;
         }
         do {
            Pieces[i].ChangePiece(Rnd.Range(0, 5), Rnd.Range(0, 5), Rnd.Range(0, 5), PieceTypes[Rnd.Range(0, 5)]);
         } while (!PB.CheckPiece(Pieces[i - 1].T, Pieces[i - 1].L, Pieces[i - 1].R, Pieces[i - 1].C, Pieces[i].L, Pieces[i].R, Pieces[i].C)); //Check that the previous piece attacks the current one
         

         for (int j = 0; j < i - 1; j++) {
            if (PB.CheckPiece(Pieces[j].T, Pieces[j].L, Pieces[j].R, Pieces[j].C, Pieces[i].L, Pieces[i].R, Pieces[i].C) || ShareExactCoordinate(Pieces[j], Pieces[i])) { //Checks if any previous piece blocks the current one, although this is done shittily so it prolly has side effects that I don't care enough to fix.
               goto RestartPiece;
            }
         }
         Timwi = 0;
      }

      FinalPieceType =  PieceTypes[(Array.IndexOf(PieceTypes, Pieces[0].T) + Array.IndexOf(PieceTypes, Pieces[6].T)) % 5]; //Follow the table from the manual.

      for (int i = 0; i < 100000; i++) {
         do {
            Eighth.ChangePiece(Rnd.Range(0, 5), Rnd.Range(0, 5), Rnd.Range(0, 5), PieceTypes[Rnd.Range(0, 5)]);         //Generates the last piece such that it attacks the first and is attacked by the last. Probably.
         } while (Eighth.T != FinalPieceType);

         for (int j = 0; j < 7; j++) {
            if (ShareExactCoordinate(Pieces[j], Eighth)) {
               continue;
            }
         }

         if (PB.CheckPiece(Eighth.T, Eighth.L, Eighth.R, Eighth.C, Pieces[0].L, Pieces[0].R, Pieces[0].C) && PB.CheckPiece(Pieces[6].T, Pieces[6].L, Pieces[6].R, Pieces[6].C, Eighth.L, Eighth.R, Eighth.C)) {
            break;
         }

         if (i == 99999) {  //Just in case.
            GeneratePuzzle();
            return;
         }
      }

      LogPieces();
   }

   bool CheckDuplicateCoordinatesForAll (Piece Input) {
      for (int i = 0; i < 7; i++) {
         if (ShareExactCoordinate(Input, Pieces[i])) {
            return true;
         }
      }
      return false;
   }

   bool ShareExactCoordinate (Piece P1, Piece P2) {
      if (P1.R == P2.R) {
         if (P1.C == P2.C) {
            if (P1.L == P2.L) {
               return true;
            }
         }
      }
      return false;
   }

   private IEnumerator KeyPress (int HiKavin) {
      CoordinateButtons[HiKavin].AddInteractionPunch(0.125f);
      Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
      for (int i = 0; i < 5; i++) {
         CoordinateButtons[HiKavin].transform.localPosition += new Vector3(0, 0, -0.005f);
         yield return new WaitForSeconds(0.005F);
      }
   }

   private IEnumerator KeyDepress (int HiKavin) {
      CoordinateButtons[HiKavin].AddInteractionPunch(0.125f);
      Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
      for (int i = 0; i < 5; i++) {
         CoordinateButtons[HiKavin].transform.localPosition += new Vector3(0, 0, +0.005f);
         yield return new WaitForSeconds(0.005F);
      }
   }

   void Solve () {
      GetComponent<KMBombModule>().HandlePass();
      ModuleSolved = true;
   }

   void Strike () {
      GetComponent<KMBombModule>().HandleStrike();
   }

#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string Command) {
      yield return null;
   }

   IEnumerator TwitchHandleForcedSolve () {
      yield return null;
   }

   public class Piece {

      public int L = 0;
      public int C = 0;
      public int R = 0;

      public string T = "Pawn";

      public Piece () {
         L = 0;
         C = 0;
         R = 0;
         T = "Pawn";
      }

      public Piece (int Layer, int Column, int Row, string PType) {  //I fucking hate making constructors.
         L = Layer;
         C = Column;
         R = Row;
         T = PType;
      }

      public void ChangePiece (int Layer, int Column, int Row, string PType) {
         L = Layer;
         C = Column;
         R = Row;
         T = PType;
      }
   }

}


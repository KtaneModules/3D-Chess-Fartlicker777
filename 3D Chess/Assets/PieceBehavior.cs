using UnityEngine;

public class PieceBehavior : MonoBehaviour {

   public bool CheckPiece (string Piece, int FromL, int FromR, int FromC, int ToL, int ToR, int ToC) {
      switch (Piece) {
         case "Knight":
            return Knight(FromL, FromR, FromC, ToL, ToR, ToC);
         case "Bishop":
            return Bishop(FromL, FromR, FromC, ToL, ToR, ToC);
         case "Rook":
            return Rook(FromL, FromR, FromC, ToL, ToR, ToC);
         case "Queen":
            return Queen(FromL, FromR, FromC, ToL, ToR, ToC);
         case "King":
            return King(FromL, FromR, FromC, ToL, ToR, ToC);
         default:
            return false;
      }
   }

   public bool Rook (int FromL, int FromR, int FromC, int ToL, int ToR, int ToC) { //For a rook to attack a piece, it needs to share exactly 2 coordinates. So XY, XZ, or YZ

      int Match = 0;
      if (FromL == ToL) {
         Match++;
      }
      if (FromR == ToR) {
         Match++;
      }
      if (FromC == ToC) {
         Match++;
      }
      if (Match == 2) {
         return true;
      }
      return false;
   }

   public bool Bishop (int FromL, int FromR, int FromC, int ToL, int ToR, int ToC) { //For a bishop to attack a piece, it needs to share exactly 1 coordinate and the difference between the other two must be the same.
      if (FromL == ToL) {
         if (((int) Mathf.Abs(FromR - ToR)) == (int) Mathf.Abs(FromC - ToC)) {
            return true;
         }
         return false;
      }
      else if (FromR == ToR) {
         if (((int) Mathf.Abs(FromC - ToC)) == (int) Mathf.Abs(FromL - ToL)) {
            return true;
         }
         return false;
      }
      else if (FromC == ToC) {
         if (((int) Mathf.Abs(FromR - ToR)) == (int) Mathf.Abs(FromL - ToL)) {
            return true;
         }
         return false;
      }
      return false;
   }

   public bool King (int FromL, int FromR, int FromC, int ToL, int ToR, int ToC) { //For a king to attack a piece, it needs to have at most two off-by-ones and not have a coordinate difference be more than 1.
      int LDif = Mathf.Abs(FromL - ToL);
      int RDif = Mathf.Abs(FromR - ToR);
      int CDif = Mathf.Abs(FromC - ToC);
      int Dif = LDif + RDif + CDif;

      if (LDif > 1 || RDif > 1 || CDif > 1 || Dif > 2) {
         return false;
      }
      
      return true;
   }

   public bool Queen (int FromL, int FromR, int FromC, int ToL, int ToR, int ToC) { //Same as bishop and rook
      return Rook(FromL, FromR, FromC, ToL, ToR, ToC) || Bishop(FromL, FromR, FromC, ToL, ToR, ToC);
   }

   public bool Knight (int FromL, int FromR, int FromC, int ToL, int ToR, int ToC) { //Must be 3 tiles off.
      if (FromL == ToL) {
         if ((Mathf.Abs(FromR - ToR) == 1 && Mathf.Abs(FromC - ToC) == 2) || (Mathf.Abs(FromC - ToC) == 1 && Mathf.Abs(FromR - ToR) == 2)) {
            return true;
         }
      }
      else if (Mathf.Abs(FromL - ToL) == 1) {
         if ((FromR == ToR && Mathf.Abs(FromC - ToC) == 2) || (FromC == ToC && Mathf.Abs(FromR - ToR) == 2)) {
            return true;
         }
      }
      else if (Mathf.Abs(FromL - ToL) == 2) {
         if ((FromR == ToR && Mathf.Abs(FromC - ToC) == 1) || (FromC == ToC && Mathf.Abs(FromR - ToR) == 1)) {
            return true;
         }
      }
      return false;
   }
}

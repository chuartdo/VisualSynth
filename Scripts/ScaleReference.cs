using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ScaleReference {

    public   enum Scale  { CHROMATIC ,
                      MAJOR,
                      MINOR ,
                      HARMONIC_MINOR,
                      MELODIC_MINOR, 
                      NATURAL_MINOR,
                      DIATONIC_MINOR,
                      AEOLIAN,
                      PHRYGIAN,
                      LOCRIAN,
                      DORIAN,
                      LYDIAN,
                      MIXOLYDIAN,
                      PENTATONIC,
                      BLUES,
                      TURKISH,
                      INDIAN
  };

    public static readonly int[] CHROMATIC_SCALE = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 },
                        MAJOR_SCALE = { 0, 2, 4, 5, 7, 9, 11 },
                        MINOR_SCALE = { 0, 2, 3, 5, 7, 8, 10 },
                        HARMONIC_MINOR_SCALE = { 0, 2, 3, 5, 7, 8, 11 },
                        MELODIC_MINOR_SCALE = { 0, 2, 3, 5, 7, 8, 9, 10, 11 }, // mix of ascend and descend
                        NATURAL_MINOR_SCALE = { 0, 2, 3, 5, 7, 8, 10 },
                        DIATONIC_MINOR_SCALE = { 0, 2, 3, 5, 7, 8, 10 },
                        AEOLIAN_SCALE = { 0, 2, 3, 5, 7, 8, 10 },
                        PHRYGIAN_SCALE = { 0, 1, 3, 5, 7, 8, 10 },
                        LOCRIAN_SCALE = { 0, 1, 3, 5, 6, 8, 10 },
                        DORIAN_SCALE = { 0, 2, 3, 5, 7, 9, 10 },
                        LYDIAN_SCALE = { 0, 2, 4, 6, 7, 9, 11 },
                        MIXOLYDIAN_SCALE = { 0, 2, 4, 5, 7, 9, 10 },
                        PENTATONIC_SCALE = { 0, 2, 4, 7, 9 },
                        BLUES_SCALE = { 0, 2, 3, 4, 5, 7, 9, 10, 11 },
                        TURKISH_SCALE = { 0, 1, 3, 5, 7, 10, 11 },
                        INDIAN_SCALE = { 0, 1, 1, 4, 5, 8, 10 };
     
     
    public static int[] getScale( int index)
    {
        Scale scale = (Scale)index;
        switch (scale)
        {
            case Scale.CHROMATIC: return CHROMATIC_SCALE;
            case Scale.MAJOR: return MAJOR_SCALE;
            case Scale.MINOR: return MINOR_SCALE;
            case Scale.HARMONIC_MINOR: return HARMONIC_MINOR_SCALE;
            case Scale.MELODIC_MINOR: return MELODIC_MINOR_SCALE; // mix of ascend and descend
            case Scale.NATURAL_MINOR: return NATURAL_MINOR_SCALE;
            case Scale.DIATONIC_MINOR: return DIATONIC_MINOR_SCALE;
            case Scale.AEOLIAN: return AEOLIAN_SCALE;
            case Scale.PHRYGIAN: return PHRYGIAN_SCALE;
            case Scale.LOCRIAN: return LOCRIAN_SCALE;
            case Scale.DORIAN: return DORIAN_SCALE;
            case Scale.LYDIAN: return LYDIAN_SCALE;
            case Scale.MIXOLYDIAN: return MIXOLYDIAN_SCALE;
            case Scale.PENTATONIC: return PENTATONIC_SCALE;
            case Scale.BLUES: return BLUES_SCALE;
            case Scale.TURKISH: return TURKISH_SCALE;
            case Scale.INDIAN: return INDIAN_SCALE;
           
        }
        return MAJOR_SCALE;
    }
    

}
 
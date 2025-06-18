export interface Course {
  id?: string;
  name: string;
  location: string;
  totalPar: number;
  totalYardage: number;
  slopeRating: number;
  courseRating: number;
  courseHoles: CourseHole[];
}

export interface CourseHole {
  id?: string;
  courseId?: string;
  holeNumber: number;
  par: number;
  yardage: number;
  handicapIndex: number;
}

export interface CourseDto {
  id?: string;
  name: string;
  location: string;
  totalPar: number;
  totalYardage: number;
  slopeRating: number;
  courseRating: number;
  courseHoles: CourseHole[];
}

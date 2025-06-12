export interface TrainingMenu {
  menuId: string;
  menuName: string;
  targetArea: string;
  description?: string;
  defaultSets?: number;
  defaultReps?: number;
  defaultWeight?: number;
}

export interface TrainingRecord {
  recordId: string;
  menuId: string;
  menuName: string;
  date: string;
  sets: TrainingSet[];
  notes?: string;
}

export interface TrainingSet {
  setNumber: number;
  reps: number;
  weight: number;
  completed: boolean;
}

export interface DailyRecord {
  date: string;
  records: TrainingRecord[];
  totalSets: number;
  totalVolume: number;
}

export interface DashboardStats {
  totalWorkouts: number;
  currentStreak: number;
  totalVolume: number;
  favoriteExercise: string;
  weeklyProgress: WeeklyProgress[];
}

export interface WeeklyProgress {
  date: string;
  volume: number;
  workouts: number;
}
export interface LeagueRules {
  id: string;
  seasonId: string;
  content: string;
  createdAt: string;
  updatedAt: string;
  createdBy?: string;
  updatedBy?: string;
}

export interface UpdateRulesRequest {
  content: string;
}

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface LeagueSettings {
    id: string;
    seasonId: string;
    handicapMethod: HandicapCalculationMethod;
    coursePar: number;
    courseRating: number;
    slopeRating: number;
    maxRoundsForHandicap: number;
    scoringMethod: ScoringMethod;
    pointsSystem: PointsSystem;
    holeWinPoints: number;
    holeHalvePoints: number;
    matchWinBonus: number;
    matchTiePoints: number;
    useSessionHandicaps: boolean;
    allowHandicapUpdates: boolean;
    customRules?: string;
    createdDate: string;
    modifiedDate?: string;
}

export enum HandicapCalculationMethod {
    WorldHandicapSystem = 0,
    SimpleAverage = 1
}

export enum ScoringMethod {
    MatchPlay = 0,
    StrokePlay = 1
}

export enum PointsSystem {
    HolePointsWithMatchBonus = 0,
    ScoreBasedPoints = 1,
    Custom = 2
}

export interface EnumOption {
    value: number;
    name: string;
}

@Injectable({
    providedIn: 'root'
})
export class LeagueSettingsService {
    private apiUrl = '/api/LeagueSettings';

    constructor(private http: HttpClient) { }

    getLeagueSettings(seasonId: string): Observable<LeagueSettings> {
        return this.http.get<LeagueSettings>(`${this.apiUrl}/season/${seasonId}`);
    }

    updateLeagueSettings(seasonId: string, settings: LeagueSettings): Observable<LeagueSettings> {
        return this.http.put<LeagueSettings>(`${this.apiUrl}/season/${seasonId}`, settings);
    }

    resetToDefaults(seasonId: string): Observable<LeagueSettings> {
        return this.http.post<LeagueSettings>(`${this.apiUrl}/season/${seasonId}/reset`, {});
    }

    getHandicapMethods(): Observable<EnumOption[]> {
        return this.http.get<EnumOption[]>(`${this.apiUrl}/enums/handicap-methods`);
    }

    getScoringMethods(): Observable<EnumOption[]> {
        return this.http.get<EnumOption[]>(`${this.apiUrl}/enums/scoring-methods`);
    }

    getPointsSystems(): Observable<EnumOption[]> {
        return this.http.get<EnumOption[]>(`${this.apiUrl}/enums/points-systems`);
    }
}

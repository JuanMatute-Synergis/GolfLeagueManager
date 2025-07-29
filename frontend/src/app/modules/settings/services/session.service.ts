import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, catchError, of } from 'rxjs';

export interface SessionInfo {
    sessionNumber: number;
    sessionStartWeekNumber: number;
    currentWeekNumber: number;
}

@Injectable({
    providedIn: 'root'
})
export class SessionService {
    private readonly apiUrl = '/api/weeks';

    constructor(private http: HttpClient) { }

    getCurrentSession(seasonId: string, weekId?: string): Observable<SessionInfo> {
        if (weekId) {
            return this.getSessionForWeek(seasonId, weekId);
        }

        // If no weekId provided, get current week first
        return this.http.get<any>(`${this.apiUrl}/season/${seasonId}/current`).pipe(
            map(currentWeek => this.calculateSessionInfo(seasonId, currentWeek))
        );
    }

    private getSessionForWeek(seasonId: string, weekId: string): Observable<SessionInfo> {
        return this.http.get<any[]>(`${this.apiUrl}/season/${seasonId}`).pipe(
            map(weeks => {
                const selectedWeek = weeks.find(w => w.id === weekId);
                if (!selectedWeek) {
                    throw new Error('Week not found');
                }
                return this.calculateSessionInfoForWeek(weeks, selectedWeek);
            })
        );
    }

    private calculateSessionInfo(seasonId: string, currentWeek: any): SessionInfo {
        // This would need to be implemented with the full weeks data
        // For now, return a basic implementation
        return {
            sessionNumber: 1,
            sessionStartWeekNumber: 1,
            currentWeekNumber: currentWeek.weekNumber
        };
    }

    private calculateSessionInfoForWeek(allWeeks: any[], selectedWeek: any): SessionInfo {
        const currentWeekNumber = selectedWeek.weekNumber;

        // Find the current session start week
        const sessionStartWeek = allWeeks
            .filter(w => w.weekNumber <= currentWeekNumber && w.sessionStart)
            .sort((a, b) => b.weekNumber - a.weekNumber)[0];

        const sessionStartWeekNumber = sessionStartWeek?.weekNumber || 1;

        // Calculate session number
        const sessionNumber = allWeeks
            .filter(w => w.weekNumber <= currentWeekNumber && w.sessionStart)
            .length || 1;

        return {
            sessionNumber,
            sessionStartWeekNumber,
            currentWeekNumber
        };
    }

    getSessionStartWeekForWeek(seasonId: string, weekNumber: number): Observable<number> {
        return this.http.get<any[]>(`${this.apiUrl}/season/${seasonId}`).pipe(
            map(weeks => {
                const sessionStartWeek = weeks
                    .filter(w => w.weekNumber <= weekNumber && w.sessionStart)
                    .sort((a, b) => b.weekNumber - a.weekNumber)[0];

                return sessionStartWeek?.weekNumber || 1;
            })
        );
    }

    getAvailableSessions(seasonId: string): Observable<SessionInfo[]> {
        console.log('SessionService: Loading sessions for season:', seasonId);
        return this.http.get<any[]>(`${this.apiUrl}/season/${seasonId}`).pipe(
            map(weeks => {
                console.log('SessionService: Received weeks data:', weeks);
                
                // Find all session start weeks
                const sessionStartWeeks = weeks
                    .filter(w => w.sessionStart === true)
                    .sort((a, b) => a.weekNumber - b.weekNumber);

                console.log('SessionService: Found session start weeks:', sessionStartWeeks);

                // Create session info for each session
                const sessions = sessionStartWeeks.map((sessionWeek, index) => ({
                    sessionNumber: index + 1,
                    sessionStartWeekNumber: sessionWeek.weekNumber,
                    currentWeekNumber: sessionWeek.weekNumber // Default to start week
                }));
                
                console.log('SessionService: Generated sessions:', sessions);
                return sessions;
            }),
            map(sessions => {
                if (sessions.length === 0) {
                    console.log('SessionService: No sessions found in data, using defaults');
                    // Provide default 3 sessions if no session data found
                    return this.getDefaultSessions();
                }
                return sessions;
            }),
            catchError(error => {
                console.error('SessionService: Failed to load sessions from API, using defaults:', error);
                return of(this.getDefaultSessions());
            })
        );
    }

    private getDefaultSessions(): SessionInfo[] {
        // Default session structure - 3 sessions of ~7 weeks each
        return [
            {
                sessionNumber: 1,
                sessionStartWeekNumber: 1,
                currentWeekNumber: 1
            },
            {
                sessionNumber: 2,
                sessionStartWeekNumber: 8,
                currentWeekNumber: 8
            },
            {
                sessionNumber: 3,
                sessionStartWeekNumber: 15,
                currentWeekNumber: 15
            }
        ];
    }
}

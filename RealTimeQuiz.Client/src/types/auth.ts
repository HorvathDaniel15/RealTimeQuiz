export type RegisterRequest = {
    email: string;
    password: string;
    userName?: string;
};

export type RegisterResult = {
    userId: string;
    email: string;
};

export type LoginRequest = {
    email: string;
    password: string;
};

export type LoginResult = {
    accessToken: string;
    refreshToken: string;
    accessTokenExpiresAtUtc: string;
};

export type RefreshRequest = {
    refreshToken: string;
};

export type RefreshResult = {
    accessToken: string;
    refreshToken: string;
    accessTokenExpiresAtUtc: string;
};

export type LogoutRequest = {
    refreshToken?: string;
    logoutAllDevices: boolean;
};

export type AuthMeResult = {
    userId: string;
    userName: string | null;
    email: string | null;
};


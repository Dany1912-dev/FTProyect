export type UserRole = 'owner' | 'admin' | 'user'

export interface User {
  id: string
  username: string
  email: string
  role: UserRole
  createdAt: string
}

export interface Folder {
  id: string
  name: string
  type: 'personal' | 'shared' | 'group'
  ownerId: string
  createdAt: string
}

export interface FileItem {
  id: string
  name: string
  size: number
  mimeType: string
  folderId: string
  uploadedBy: string
  createdAt: string
}

export interface GroupMember {
  userId: string
  folderId: string
  role: 'editor' | 'viewer'
}

export interface ApiResponse<T> {
  data: T
  message?: string
}

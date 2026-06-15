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
  ownerUsername: string
  createdAt: string
}

export interface FileItem {
  id: string
  name: string
  size: number
  mimeType: string
  folderId: string
  uploadedBy: string | null
  createdAt: string
}

export interface FolderMemberInfo {
  userId: string
  username: string
  role: 'editor' | 'viewer'
}

export interface FolderContents {
  folder?: Folder
  folders: Folder[]
  files: FileItem[]
  myRole?: 'owner' | 'editor' | 'viewer'
}

export interface ApiResponse<T> {
  data: T
  message?: string
}

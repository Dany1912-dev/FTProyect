export type UserRole = 'owner' | 'admin' | 'user'

export interface User {
  id: string
  username: string
  email: string
  role: UserRole
  createdAt: string
  storageUsedBytes: number
  storageQuotaBytes: number
}

export interface Folder {
  id: string
  name: string
  type: 'personal' | 'shared' | 'group'
  ownerId: string
  ownerUsername: string
  parentFolderId: string | null
  createdAt: string
  deletedAt: string | null
}

export interface FolderTreeNode {
  id: string
  name: string
  parentFolderId: string | null
}

export interface FileItem {
  id: string
  name: string
  size: number
  mimeType: string
  folderId: string
  uploadedBy: string | null
  createdAt: string
  deletedAt: string | null
}

export interface FolderMemberInfo {
  userId: string
  username: string
  role: 'editor' | 'viewer'
}

export interface FolderContents {
  folder?: Folder
  path: Folder[]
  folders: Folder[]
  files: FileItem[]
  myRole?: 'owner' | 'editor' | 'viewer'
}

export interface TrashContents {
  folders: Folder[]
  files: FileItem[]
}

export interface ApiResponse<T> {
  data: T
  message?: string
}
